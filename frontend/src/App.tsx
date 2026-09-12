import type { ReactNode } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout';
import { useAuth } from './contexts/AuthContext';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import Studios from './pages/Studios';
import Kilns from './pages/Kilns';
import Schedules from './pages/Schedules';
import LoadBatches from './pages/LoadBatches';

function PrivateRoute({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  if (!user) return <Navigate to="/login" replace />;
  return <>{children}</>;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route
        path="/"
        element={
          <PrivateRoute>
            <Layout />
          </PrivateRoute>
        }
      >
        <Route index element={<Dashboard />} />
        <Route path="studios" element={<Studios />} />
        <Route path="kilns" element={<Kilns />} />
        <Route path="schedules" element={<Schedules />} />
        <Route path="load-batches" element={<LoadBatches />} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
