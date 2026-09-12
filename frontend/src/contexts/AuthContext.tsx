import React, { createContext, useContext, useMemo, useState } from 'react';
import { api, clearAuth, loadToken, loadUser, saveAuth, User } from '../api/client';

type AuthContextValue = {
  user: User | null;
  token: string | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(() => loadUser());
  const [token, setToken] = useState<string | null>(() => loadToken());

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      token,
      async login(username, password) {
        const res = await api.login(username, password);
        saveAuth(res.token, res.user);
        setToken(res.token);
        setUser(res.user);
      },
      logout() {
        clearAuth();
        setToken(null);
        setUser(null);
      },
    }),
    [user, token]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
