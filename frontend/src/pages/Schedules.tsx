import { FormEvent, useEffect, useState } from 'react';
import { api, Kiln, Schedule, Segment } from '../api/client';

type FormState = {
  kilnId: string;
  name: string;
  coneOrTarget: string;
  status: string;
  segments: Segment[];
};

const emptySeg = (): Segment => ({
  seq: 1,
  rampCPerHour: 80,
  holdMinutes: 0,
  targetTempC: 1000,
});

const emptyForm = (): FormState => ({
  kilnId: '',
  name: '',
  coneOrTarget: '',
  status: 'draft',
  segments: [emptySeg()],
});

const statusLabel: Record<string, string> = {
  draft: '草稿',
  approved: '已批准',
  retired: '已退役',
};

export default function Schedules() {
  const [list, setList] = useState<Schedule[]>([]);
  const [kilns, setKilns] = useState<Kiln[]>([]);
  const [form, setForm] = useState<FormState>(emptyForm());
  const [editing, setEditing] = useState<Schedule | null>(null);
  const [open, setOpen] = useState(false);
  const [error, setError] = useState('');

  async function load() {
    const [schedules, kilnList] = await Promise.all([api.getSchedules(), api.getKilns()]);
    setList(schedules);
    setKilns(kilnList);
  }

  useEffect(() => {
    load().catch((e) => setError(e.message));
  }, []);

  function openCreate() {
    setEditing(null);
    const f = emptyForm();
    f.kilnId = kilns[0]?.id?.toString() ?? '';
    setForm(f);
    setOpen(true);
  }

  function openEdit(item: Schedule) {
    setEditing(item);
    setForm({
      kilnId: String(item.kilnId),
      name: item.name,
      coneOrTarget: item.coneOrTarget,
      status: item.status,
      segments: item.segments.map((s) => ({
        seq: s.seq,
        rampCPerHour: s.rampCPerHour,
        holdMinutes: s.holdMinutes,
        targetTempC: s.targetTempC,
      })),
    });
    setOpen(true);
  }

  function updateSegment(index: number, patch: Partial<Segment>) {
    setForm((prev) => {
      const segments = [...prev.segments];
      segments[index] = { ...segments[index], ...patch };
      return { ...prev, segments };
    });
  }

  function addSegment() {
    setForm((prev) => ({
      ...prev,
      segments: [
        ...prev.segments,
        {
          seq: prev.segments.length + 1,
          rampCPerHour: 60,
          holdMinutes: 0,
          targetTempC: 1200,
        },
      ],
    }));
  }

  function removeSegment(index: number) {
    setForm((prev) => ({
      ...prev,
      segments: prev.segments
        .filter((_, i) => i !== index)
        .map((s, i) => ({ ...s, seq: i + 1 })),
    }));
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    try {
      const body = {
        kilnId: Number(form.kilnId),
        name: form.name,
        coneOrTarget: form.coneOrTarget,
        status: form.status,
        segments: form.segments.map((s, i) => ({
          seq: i + 1,
          rampCPerHour: Number(s.rampCPerHour),
          holdMinutes: Number(s.holdMinutes),
          targetTempC: Number(s.targetTempC),
        })),
      };
      if (editing) await api.updateSchedule(editing.id, body);
      else await api.createSchedule(body);
      setOpen(false);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : '保存失败');
    }
  }

  async function onDelete(id: number) {
    if (!confirm('确定删除该烧成制度？')) return;
    try {
      await api.deleteSchedule(id);
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : '删除失败');
    }
  }

  return (
    <>
      <div className="topbar">
        <h1>烧成制度</h1>
        <button className="btn" onClick={openCreate}>
          新建制度
        </button>
      </div>
      {error && <div className="error">{error}</div>}
      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>名称</th>
              <th>窑炉</th>
              <th>锥号/目标</th>
              <th>曲线段数</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            {list.map((item) => (
              <tr key={item.id}>
                <td>{item.name}</td>
                <td>{item.kilnCode}</td>
                <td>{item.coneOrTarget}</td>
                <td>{item.segments?.length ?? 0}</td>
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
            <h2>{editing ? '编辑烧成制度' : '新建烧成制度'}</h2>
            <form onSubmit={onSubmit}>
              <div className="form-grid">
                <label>
                  窑炉
                  <select
                    value={form.kilnId}
                    onChange={(e) => setForm({ ...form, kilnId: e.target.value })}
                    required
                  >
                    <option value="">请选择</option>
                    {kilns.map((k) => (
                      <option key={k.id} value={k.id}>
                        {k.kilnCode}（{k.studioName}）
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  名称
                  <input
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    required
                  />
                </label>
                <label>
                  锥号/目标
                  <input
                    value={form.coneOrTarget}
                    onChange={(e) => setForm({ ...form, coneOrTarget: e.target.value })}
                    required
                  />
                </label>
                <label>
                  状态
                  <select
                    value={form.status}
                    onChange={(e) => setForm({ ...form, status: e.target.value })}
                  >
                    <option value="draft">草稿</option>
                    <option value="approved">已批准</option>
                    <option value="retired">已退役</option>
                  </select>
                </label>
              </div>

              <div className="segments">
                <div className="topbar" style={{ marginBottom: 8 }}>
                  <strong>曲线段</strong>
                  <button type="button" className="btn secondary" onClick={addSegment}>
                    添加段
                  </button>
                </div>
                <table>
                  <thead>
                    <tr>
                      <th>序号</th>
                      <th>升温(°C/h)</th>
                      <th>保温(分)</th>
                      <th>目标(°C)</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    {form.segments.map((seg, i) => (
                      <tr key={i}>
                        <td>{i + 1}</td>
                        <td>
                          <input
                            type="number"
                            value={seg.rampCPerHour}
                            onChange={(e) =>
                              updateSegment(i, { rampCPerHour: Number(e.target.value) })
                            }
                            required
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            value={seg.holdMinutes}
                            onChange={(e) =>
                              updateSegment(i, { holdMinutes: Number(e.target.value) })
                            }
                            required
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            value={seg.targetTempC}
                            onChange={(e) =>
                              updateSegment(i, { targetTempC: Number(e.target.value) })
                            }
                            required
                          />
                        </td>
                        <td>
                          {form.segments.length > 1 && (
                            <button
                              type="button"
                              className="btn danger"
                              onClick={() => removeSegment(i)}
                            >
                              删
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
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
