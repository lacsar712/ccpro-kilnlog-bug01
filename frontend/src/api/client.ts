const TOKEN_KEY = 'kilnlog_token';
const USER_KEY = 'kilnlog_user';

export type User = {
  id: number;
  username: string;
  role: string;
};

export type Studio = {
  id: number;
  name: string;
  city: string;
  notes: string | null;
  kilnCount: number;
};

export type Kiln = {
  id: number;
  studioId: number;
  studioName?: string | null;
  kilnCode: string;
  maxTempC: number;
  fuelType: string;
  status: string;
};

export type Segment = {
  id?: number;
  seq: number;
  rampCPerHour: number;
  holdMinutes: number;
  targetTempC: number;
};

export type Schedule = {
  id: number;
  kilnId: number;
  kilnCode?: string | null;
  name: string;
  coneOrTarget: string;
  status: string;
  segments: Segment[];
};

export type LoadBatch = {
  id: number;
  kilnId: number;
  kilnCode?: string | null;
  scheduleId: number;
  scheduleName?: string | null;
  loadDate: string;
  pieceCount: number;
  glazeNotes: string | null;
  status: string;
};

export type DashboardStats = {
  kilnCount: number;
  firingKilnCount: number;
  monthLoadBatchCount: number;
  approvedScheduleCount: number;
};

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem(TOKEN_KEY);
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> | undefined),
  };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(path, { ...options, headers });
  const data = await res.json().catch(() => ({}));
  if (!res.ok) {
    throw new Error((data as { message?: string }).message || `请求失败 (${res.status})`);
  }
  return data as T;
}

export const api = {
  login: (username: string, password: string) =>
    request<{ token: string; user: User }>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),
  me: () => request<User>('/api/auth/me'),
  dashboard: () => request<DashboardStats>('/api/dashboard'),

  getStudios: () => request<Studio[]>('/api/studios'),
  createStudio: (body: unknown) =>
    request<Studio>('/api/studios', { method: 'POST', body: JSON.stringify(body) }),
  updateStudio: (id: number, body: unknown) =>
    request<Studio>(`/api/studios/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteStudio: (id: number) =>
    request(`/api/studios/${id}`, { method: 'DELETE' }),

  getKilns: (studioId?: number) =>
    request<Kiln[]>(studioId ? `/api/kilns?studioId=${studioId}` : '/api/kilns'),
  createKiln: (body: unknown) =>
    request<Kiln>('/api/kilns', { method: 'POST', body: JSON.stringify(body) }),
  updateKiln: (id: number, body: unknown) =>
    request<Kiln>(`/api/kilns/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteKiln: (id: number) =>
    request(`/api/kilns/${id}`, { method: 'DELETE' }),

  getSchedules: (kilnId?: number) =>
    request<Schedule[]>(kilnId ? `/api/firing-schedules?kilnId=${kilnId}` : '/api/firing-schedules'),
  createSchedule: (body: unknown) =>
    request<Schedule>('/api/firing-schedules', { method: 'POST', body: JSON.stringify(body) }),
  updateSchedule: (id: number, body: unknown) =>
    request<Schedule>(`/api/firing-schedules/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteSchedule: (id: number) =>
    request(`/api/firing-schedules/${id}`, { method: 'DELETE' }),

  getLoadBatches: (kilnId?: number) =>
    request<LoadBatch[]>(kilnId ? `/api/load-batches?kilnId=${kilnId}` : '/api/load-batches'),
  createLoadBatch: (body: unknown) =>
    request<LoadBatch>('/api/load-batches', { method: 'POST', body: JSON.stringify(body) }),
  updateLoadBatch: (id: number, body: unknown) =>
    request<LoadBatch>(`/api/load-batches/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deleteLoadBatch: (id: number) =>
    request(`/api/load-batches/${id}`, { method: 'DELETE' }),
};

export function saveAuth(token: string, user: User) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearAuth() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function loadUser(): User | null {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as User;
  } catch {
    return null;
  }
}

export function loadToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}
