export interface LoginResponse {
  token: string;
  role: 'Dispatcher' | 'Technician';
  name: string;
}

export interface EventDto {
  id: string;
  title: string;
  description: string;
  location?: string;
  status: string;
  priority: string;
  assignedTechnicianId?: string | null;
  createdAt: string;
  updatedAt: string;
}
