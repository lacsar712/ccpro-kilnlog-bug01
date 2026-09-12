import { NavLink, Navigate, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const links = [
  { to: '/', label: '仪表盘', end: true },
  { to: '/studios', label: '工作室' },
  { to: '/kilns', label: '窑炉' },
  { to: '/schedules', label: '烧成制度' },
  { to: '/load-batches', label: '装窑批次' },
];

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  if (!user) return <Navigate to="/login" replace />;

  return (
    <div className="layout">
      <aside className="sidebar">
        <div className="brand">
          KilnLog
          <small>窑炉装窑与烧成曲线台账</small>
        </div>
        {links.map((l) => (
          <NavLink
            key={l.to}
            to={l.to}
            end={l.end}
            className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}
          >
            {l.label}
          </NavLink>
        ))}
        <div style={{ marginTop: 'auto', paddingTop: 24 }} />
        <div style={{ fontSize: '0.85rem', opacity: 0.85 }}>
          {user.username}（{user.role === 'admin' ? '管理员' : '陶艺师'}）
        </div>
        <button
          className="btn secondary"
          style={{ marginTop: 8 }}
          onClick={() => {
            logout();
            navigate('/login');
          }}
        >
          退出登录
        </button>
      </aside>
      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
