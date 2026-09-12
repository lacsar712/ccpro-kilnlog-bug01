import { FormEvent, useEffect, useMemo, useState } from 'react';
import { api, Kiln, LoadBatch, Schedule } from '../api/client';

const emptyForm = {
  kilnId: '',
  scheduleId: '',
  loadDate: new Date().toISOString().slice(0, 10),
  pieceCount: '12',
  glazeNotes: '',
  status: 'planned',
};

const statusLabel: Record<string, string> = {
  planned: '计划',
  loaded: '已装窑',
  fired: '已烧成',
  unloaded: '已出窑',
};

export default function LoadBatches() {
  const [list, setList] = useState<LoadBatch[]>([]);
  const [kilns, setKilns] = useState<Kiln[]>([]);
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [editing, setEditing] = useState<LoadBatch | null>(null);
  const [open, setOpen] = useState(false);
  const [error, setError] = useState('');

  async function load() {
    const [batches, kilnList, scheduleList] = await Promise.all([
      api.getLoadBatches(),
      api.getKilns(),
      api.getSchedules(),
    ]);
    setList(batches);
    setKilns(kilnList);
    setSchedules(scheduleList);
  }

  useEffect(() => {
    load().catch((e) => setError(e.message));
  }, []);

  const filteredSchedules = useMemo(
    () =>
      form.kilnId
        ? schedules.filter((s) => s.kilnId === Number(form.kilnId))
        : schedules,
    [schedules, form.kilnId]
  );

  function openCreate() {
    setEditing(null);
    const kilnId = kilns[0]?.id?.toString() ?? '';
    const related = schedules.filter((s) => s.kilnId === Number(kilnId));
    setForm({
      ...emptyForm,
      kilnId,
      scheduleId: related[0]?.id?.toString() ?? '',
      loadDate: new Date().toISOString().slice(0, 10),
    });
    setOpen(true);
  }

  function openEdit(item: LoadBatch) {
    setEditing(item);
    setForm({
      kilnId: String(item.kilnId),
      scheduleId: String(item.scheduleId),
      loadDate: item.loadDate.slice(0, 10),
      pieceCount: String(item.pieceCount),
      glazeNotes: item.glazeNotes ?? '',
      status: item.status,
    });
    setOpen(true);
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    try {
      const body = {
        kilnId: Number(form.kilnId),
        scheduleId: Number(form.scheduleId),
        loadDate: form.loadDate,
        pieceCount: Number(form.pieceCount),
        glazeNotes: form.glazeNotes || null,
        status: form.status,
      };
      if (editing) await api.updateLoadBatch(editing.id, body);
      else await api.createLoadBatch(body);
      setOpen(false);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : '保存失败');
    }
  }

  async function onDelete(id: number) {
    if (!confirm('确定删除该装窑批次？')) return;
    await api.deleteLoadBatch(id);
    await load();
  }

  return (
    <>
      <div className="topbar">
        <h1>装窑批次</h1>
        <button className="btn" onClick={openCreate}>
          新建批次
        </button>
      </div>
      {error && <div className="error">{error}</div>}
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>装窑日</th>
              <th>窑炉</th>
              <th>制度</th>
              <th>件数</th>
              <th>釉料备注</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            {list.map((item) => (
              <tr key={item.id}>
                <td>{item.loadDate.slice(0, 10)}</td>
                <td>{item.kilnCode}</td>
                <td>{item.scheduleName}</td>
                <td>{item.pieceCount}</td>
                <td>{item.glazeNotes || '—'}</td>
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
                <td colSpan={7} className="muted">
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
            <h2>{editing ? '编辑装窑批次' : '新建装窑批次'}</h2>
            <form onSubmit={onSubmit}>
              <div className="form-grid">
                <label>
                  窑炉
                  <select
                    value={form.kilnId}
                    onChange={(e) => {
                      const kilnId = e.target.value;
                      const related = schedules.filter((s) => s.kilnId === Number(kilnId));
                      setForm({
                        ...form,
                        kilnId,
                        scheduleId: related[0]?.id?.toString() ?? '',
                      });
                    }}
                    required
                  >
                    <option value="">请选择</option>
                    {kilns.map((k) => (
                      <option key={k.id} value={k.id}>
                        {k.kilnCode}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  烧成制度
                  <select
                    value={form.scheduleId}
                    onChange={(e) => setForm({ ...form, scheduleId: e.target.value })}
                    required
                  >
                    <option value="">请选择</option>
                    {filteredSchedules.map((s) => (
                      <option key={s.id} value={s.id}>
                        {s.name}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  装窑日
                  <input
                    type="date"
                    value={form.loadDate}
                    onChange={(e) => setForm({ ...form, loadDate: e.target.value })}
                    required
                  />
                </label>
                <label>
                  件数
                  <input
                    type="number"
                    value={form.pieceCount}
                    onChange={(e) => setForm({ ...form, pieceCount: e.target.value })}
                    required
                  />
                </label>
                <label>
                  状态
                  <select
                    value={form.status}
                    onChange={(e) => setForm({ ...form, status: e.target.value })}
                  >
                    <option value="planned">计划</option>
                    <option value="loaded">已装窑</option>
                    <option value="fired">已烧成</option>
                    <option value="unloaded">已出窑</option>
                  </select>
                </label>
                <label style={{ gridColumn: '1 / -1' }}>
                  釉料备注
                  <textarea
                    rows={3}
                    value={form.glazeNotes}
                    onChange={(e) => setForm({ ...form, glazeNotes: e.target.value })}
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
