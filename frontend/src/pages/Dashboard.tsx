import { useEffect, useState } from 'react';
import { api, DashboardStats } from '../api/client';

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    api
      .dashboard()
      .then(setStats)
      .catch((e) => setError(e.message));
  }, []);

  return (
    <>
      <div className="topbar">
        <h1>仪表盘</h1>
      </div>
      {error && <div className="error">{error}</div>}
      <div className="stats">
        <div className="stat">
          <div className="label">窑炉总数</div>
          <div className="value">{stats?.kilnCount ?? '—'}</div>
        </div>
        <div className="stat">
          <div className="label">烧成中窑炉</div>
          <div className="value">{stats?.firingKilnCount ?? '—'}</div>
        </div>
        <div className="stat">
          <div className="label">本月装窑批次</div>
          <div className="value">{stats?.monthLoadBatchCount ?? '—'}</div>
        </div>
        <div className="stat">
          <div className="label">已批准制度</div>
          <div className="value">{stats?.approvedScheduleCount ?? '—'}</div>
        </div>
      </div>
      <div className="card" style={{ marginTop: 16 }}>
        <h3 style={{ marginTop: 0 }}>说明</h3>
        <p className="muted">
          本系统用于陶艺工作室窑炉台账：登记工作室与窑炉、维护烧成曲线段制度、记录装窑批次。非预约、非电商。
          「本月装窑批次」按装窑日（loadDate）落在当月（UTC 日历月）统计。
        </p>
      </div>
    </>
  );
}
