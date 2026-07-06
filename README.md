# Field Event Management System

מערכת לניהול אירועי שטח בזמן אמת — מרגע כניסת האירוע ועד סגירתו, עם תקשורת רציפה בין סדרן לטכנאי.

> מסמך הארכיטקטורה המלא (החלטות, trade-offs, תרשימים, ERD, State Machine): [`docs/architecture.md`](docs/architecture.md).

---

## מבנה הפתרון

```
field-event-management/
├─ src/
│  ├─ Domain/          # ליבה: Entities, Enums, State Machine — ללא תלות בתשתית
│  ├─ Application/     # use-cases + interfaces (ports)
│  ├─ Contracts/       # DTOs משותפים (Agent↔Server)
│  ├─ Infrastructure/  # EF Core, JWT, hashing, WebPush-stub — מימוש ה-ports
│  ├─ Server.Api/      # ASP.NET Core: REST ingestion + SignalR Hub + Auth
│  └─ Agent.Worker/    # Webhook נכנס + Outbox (SQLite) + שולח רקע לשרת
├─ tests/Domain.Tests/ # Unit Tests ל-State Machine (xUnit)
├─ client/             # Angular 20 (סדרן + טכנאי)
└─ docker-compose.yml  # SQL Server (חלופה ל-LocalDB)
```

## דרישות מוקדמות

- **.NET 10 SDK**
- **Node.js 20.19+** ו-npm (ל-Angular 20)
- **מסד נתונים** — אחת משתיים:
  - **SQL Server LocalDB** (מותקן עם Visual Studio) — ברירת המחדל, לא דורש כלום.
  - **Docker** — `docker-compose up -d` מרים SQL Server (עדכן אז את connection string).

---

## הרצה — 3 טרמינלים

### 1. השרת המרכזי
```bash
cd src/Server.Api
dotnet run
```
- עולה על `http://localhost:5125`.
- ב-startup: מריץ EF migrations ל-LocalDB **וזורע** משתמשי דמו + מקור אירועים.

### 2. ה-Agent
```bash
cd src/Agent.Worker
dotnet run
```
- עולה על `http://localhost:5240`, יוצר Outbox מקומי (`agent-outbox.db`).

### 3. ה-Frontend
```bash
cd client
npm install      # פעם ראשונה בלבד
npx ng serve
```
- עולה על `http://localhost:4200`.

### משתמשי דמו
| תפקיד | שם משתמש | סיסמה |
|---|---|---|
| סדרן | `dispatcher` | `Passw0rd!` |
| טכנאי | `tech1` | `Passw0rd!` |

---

## הדגמת ה-Flow המלא (E2E)

1. פתח `http://localhost:4200`, התחבר כ-**dispatcher** → לוח הסדרן נפתח (מציג "real-time מחובר").
2. שלח אירוע חתום ל-Agent (המקטע `demo` מתועד למטה). האירוע יופיע בלוח הסדרן **מיידית, ללא רענון**.

### שליחת אירוע חתום (PowerShell)
```powershell
$body = '{"title":"דלת פרוצה","description":"חיישן זיהה פתיחה","location":"בניין A","priority":2}'
$ts = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
$hmac = New-Object System.Security.Cryptography.HMACSHA256 (,[Text.Encoding]::UTF8.GetBytes("demo-source-secret"))
$sig = ([BitConverter]::ToString($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$ts.$body"))) -replace '-','').ToLower()
Invoke-RestMethod -Uri 'http://localhost:5240/ingest' -Method Post `
  -Body ([Text.Encoding]::UTF8.GetBytes($body)) -ContentType 'application/json' `
  -Headers @{ 'X-Api-Key'='demo-source-key'; 'X-Timestamp'=$ts; 'X-Signature'=$sig }
```

**מה קורה מאחורי הקלעים:**
`מקור → Agent (אימות HMAC) → Outbox (SQLite) → שולח רקע → שרת (X-Agent-Key) → SQL Server → SignalR → לוח הסדרן`

### בדיקת עמידות בכשל (אירוע לא נאבד)
עצור את השרת, שלח אירוע ל-Agent (יתקבל 202 ויישמר ב-Outbox), הפעל את השרת מחדש — הרקע ישדר אוטומטית והאירוע יופיע. ה-`IdempotencyKey` מונע כפילות.

---

## בדיקות
```bash
dotnet test
```
מריץ את בדיקות ה-State Machine (מעברים חוקיים/לא-חוקיים, רישום היסטוריה, מצבים סופיים).

---

## אבטחה — בקצרה
- **משתמשים:** JWT Bearer + הרשאות role-based **נאכפות בשרת** (`[Authorize(Roles=...)]`).
- **מקור→Agent:** API Key + חתימת HMAC על הגוף + timestamp נגד replay.
- **Agent→שרת:** מפתח פנימי `X-Agent-Key`.
- **סודות:** בפרודקשן דרך user-secrets/env. ערכי הדמו ב-`appsettings.Development.json` בלבד.

---

## שימוש בכלי AI (שקיפות — חלק מהמבחן)

הפתרון פותח בעזרת **Claude (Anthropic)** ככלי עזר, תוך שיקול דעת ביקורתי:

- **מה שהוצע והתקבל:** מבנה Clean Architecture בשכבות, תבנית Outbox לאמינות, שימוש ב-SignalR ל-real-time, PBKDF2 ל-hashing.
- **מה ששונה מההצעות:** מנגנון Agent→שרת שונה מ-**gRPC ל-REST** לאחר בחינה — gRPC הוסיף מורכבות (`.proto`, codegen) ללא תועלת אמיתית לנפח ההודעות הקטן; REST עם DataAnnotations נותן validation מספק. זו החלטה מנומקת, לא ברירת מחדל של הכלי.
- **מה שנדחה:** RabbitMQ כ-broker (overkill לנפח קטן, רכיב תפעולי מיותר); FCM ל-push (תלות ב-Google) לטובת Web Push תקני; Docker ל-DB כברירת מחדל לטובת LocalDB (זמין מיידית על סביבת הפיתוח).
- כל החלטה ארכיטקטונית נבחנה מול הדרישות הספציפיות ומתועדת ב-[`docs/architecture.md`](docs/architecture.md).


## פקודות
cd C:\Users\elishevas\Desktop\test\field-event-management\src\Server.Api
dotnet run

cd C:\Users\elishevas\Desktop\test\field-event-management\src\Agent.Worker
dotnet run

cd C:\Users\elishevas\Desktop\test\field-event-management\client
npx ng serve

ps:from above

SQLite:
dotnet run read-outbox.cs

(localdb)\MSSQLLocalDB