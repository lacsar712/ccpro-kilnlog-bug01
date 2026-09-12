import { FormEvent, useEffect, useState } from 'react';
import { api, Studio } from '../api/client';

const emptyForm = { name: '', city: '', notes: '' };

export default function Studios() {
  const [list, setList] = useState<Studio[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [editing, setEditing] = useState<Studio | null>(null);
  const [open, setOpen] = useState(false);
  const [error, setError] = useState('');

  async function load() {
    setList(await api.getStudios());
  }

  useEffect(() => {
    load().catch((e) => setError(e.message));
  }, []);

  function openCreate() {
    setEditing(null);
    setForm(emptyForm);
    setOpen(true);
  }

  function openEdit(item: Studio) {
    setEditing(item);
    setForm({ name: item.name, city: item.city, notes: item.notes ?? '' });
    setOpen(true);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    try {
      const body = { name: form.name, city: form.city, notes: form.notes || null };
      if (editing) await api.updateStudio(editing.id, body);
      else await api.createStudio(body);
      setOpen(false);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : '保存失败');
    }
  }

  async function onDelete(id: number) {
    if (!confirm('确定删除该工作室及其窑炉/制度？')) return;
    await api.deleteStudio(id);
    await load();
  }

  return (
    <>
      <div className="topbar">
        <h1>工作室</h1>
        <button className="btn" onClick={openCreate}>
          新建工作室
        </button>
      </div>
      {error && <div className="error">{error}</div>}
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>名称</th>
              <th>城市</th>
              <th>窑炉数</th>
              <th>备注</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            {list.map((item) => (
              <tr key={item.id}>
                <td>{item.name}</td>
                <td>{item.city}</td>
                <td>{item.kilnCount}</td>
                <td>{item.notes || '—'}</td>
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
                <td colSpan={5} className="muted">
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
            <h2>{editing ? '编辑工作室' : '新建工作室'}</h2>
            <form onSubmit={onSubmit}>
              <div className="form-grid">
                <label>
                  名称
                  <input
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    required
                  />
                </label>
                <label>
                  城市
                  <input
                    value={form.city}
                    onChange={(e) => setForm({ ...form, city: e.target.value })}
                    required
                  />
                </label>
                <label style={{ gridColumn: '1 / -1' }}>
                  备注
                  <textarea
                    rows={3}
                    value={form.notes}
                    onChange={(e) => setForm({ ...form, notes: e.target.value })}
                  />
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
