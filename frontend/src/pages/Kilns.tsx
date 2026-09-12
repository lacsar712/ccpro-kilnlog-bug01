import { FormEvent, useEffect, useState } from 'react';
import { api, Kiln, Studio } from '../api/client';

const emptyForm = {
  studioId: '',
  kilnCode: '',
  maxTempC: '1280',
  fuelType: 'electric',
  status: 'idle',
};

const fuelLabel: Record<string, string> = {
  electric: '电窑',
  gas: '气窑',
  wood: '柴窑',
};

const statusLabel: Record<string, string> = {
  idle: '空闲',
  firing: '烧成中',
  cooling: '冷却中',
};

export default function Kilns() {
  const [list, setList] = useState<Kiln[]>([]);
  const [studios, setStudios] = useState<Studio[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [editing, setEditing] = useState<Kiln | null>(null);
  const [open, setOpen] = useState(false);
  const [error, setError] = useState('');

  async function load() {
    const [kilns, studioList] = await Promise.all([api.getKilns(), api.getStudios()]);
    setList(kilns);
    setStudios(studioList);
  }

  useEffect(() => {
    load().catch((e) => setError(e.message));
  }, []);

  function openCreate() {
    setEditing(null);
    setForm({
      ...emptyForm,
      studioId: studios[0]?.id?.toString() ?? '',
    });
    setOpen(true);
  }

  function openEdit(item: Kiln) {
    setEditing(item);
    setForm({
      studioId: String(item.studioId),
      kilnCode: item.kilnCode,
      maxTempC: String(item.maxTempC),
      fuelType: item.fuelType,
      status: item.status,
    });
    setOpen(true);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    try {
      const body = {
        studioId: Number(form.studioId),
        kilnCode: form.kilnCode,
        maxTempC: Number(form.maxTempC),
        fuelType: form.fuelType,
        status: form.status,
      };
      if (editing) await api.updateKiln(editing.id, body);
      else await api.createKiln(body);
      setOpen(false);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : '保存失败');
    }
  }

  async function onDelete(id: number) {
    if (!confirm('确定删除该窑炉？')) return;
    try {
      await api.deleteKiln(id);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : '删除失败');
    }
  }

  return (
    <>
      <div className="topbar">
        <h1>窑炉</h1>
        <button className="btn" onClick={openCreate}>
          新建窑炉
        </button>
      </div>
      {error && <div className="error">{error}</div>}
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>编号</th>
              <th>工作室</th>
              <th>最高温度(°C)</th>
              <th>燃料</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            {list.map((item) => (
              <tr key={item.id}>
                <td>{item.kilnCode}</td>
                <td>{item.studioName}</td>
                <td>{item.maxTempC}</td>
                <td>{fuelLabel[item.fuelType] ?? item.fuelType}</td>
                <td>
                  <span className={`badge ${item.status}`}>
                    {statusLabel[item.status] ?? item.status}
                  </span>
                </td>
                <td>
                  <button className="btn secondary" onClick={() => openEdit(item)}>
                    编辑
                  </button>
                  <button className="btn danger" onClick={() => onDelete(item.id)}>
                    删除
                  </button>
                </td>
              </tr>
            ))}
            {list.length === 0 && (
              <tr>
                <td colSpan={6} className="muted">
                  暂无数据
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {open && (
        <div className="modal-backdrop" onClick={() => setOpen(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <h2>{editing ? '编辑窑炉' : '新建窑炉'}</h2>
            <form onSubmit={onSubmit}>
              <div className="form-grid">
                <label>
                  工作室
                  <select
                    value={form.studioId}
                    onChange={(e) => setForm({ ...form, studioId: e.target.value })}
                    required
                  >
                    <option value="">请选择</option>
                    {studios.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  窑炉编号
                  <input
                    value={form.kilnCode}
                    onChange={(e) => setForm({ ...form, kilnCode: e.target.value })}
                    required
                  />
                </label>
                <label>
                  最高温度(°C)
                  <input
                    type="number"
                    value={form.maxTempC}
                    onChange={(e) => setForm({ ...form, maxTempC: e.target.value })}
                    required
                  />
                </label>
                <label>
                  燃料类型
                  <select
                    value={form.fuelType}
                    onChange={(e) => setForm({ ...form, fuelType: e.target.value })}
                  >
                    <option value="electric">电窑</option>
                    <option value="gas">气窑</option>
                    <option value="wood">柴窑</option>
                  </select>
                </label>
                <label>
                  状态
                  <select
                    value={form.status}
                    onChange={(e) => setForm({ ...form, status: e.target.value })}
                  >
                    <option value="idle">空闲</option>
                    <option value="firing">烧成中</option>
                    <option value="cooling">冷却中</option>
                  </select>
                </label>
              </div>
              <div className="modal-actions">
                <button type="button" className="btn secondary" onClick={() => setOpen(false)}>
                  取消
                </button>
                <button type="submit" className="btn">
                  保存
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </>
  );
}
