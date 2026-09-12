# KilnLog · 窑炉装窑批次与烧成曲线台账

陶艺工作室内部台账系统：管理工作室、窑炉、烧成制度（含曲线段）与装窑批次。**非预约系统、非电商。**

## 技术栈

- **后端**: C# .NET 8 Web API + EF Core + JWT（`Microsoft.AspNetCore.Authentication.JwtBearer`）+ BCrypt.Net
- **前端**: React 18 + Vite + TypeScript + React Router
- **数据库**: PostgreSQL 15
- **部署**: Docker Compose（后端 `mcr.microsoft.com/dotnet/sdk:8.0` 多阶段发布）

## 一键启动

在本目录执行：

```bash
docker compose up --build
```

启动时后端会自动 **Migrate** 并 **Seed** 演示数据。

| 服务 | 地址 |
|------|------|
| 前端 | http://localhost:3700 |
| 后端 API | http://localhost:8700/api |
| Postgres | localhost:5436（用户/库/密码均为 `kilnlog`） |

停止：

```bash
docker compose down
```

清空数据后重建：

```bash
docker compose down -v
docker compose up --build
```

## 测试账号

| 用户名 | 密码 | 角色 |
|--------|------|------|
| `admin` | `123456` | 管理员 |
| `potter` | `123456` | 陶艺师 |

## 功能模块

1. **Auth** — 登录签发 JWT；受保护接口需 `Authorization: Bearer <token>`
2. **Studio 工作室** — name / city / notes
3. **Kiln 窑炉** — studioId、kilnCode、maxTempC、fuelType（electric|gas|wood）、status（idle|firing|cooling）；**同工作室 kilnCode 唯一**
4. **FiringSchedule 烧成制度** — kilnId、name、coneOrTarget、status（draft|approved|retired）；子表 **ScheduleSegment**：seq、rampCPerHour、holdMinutes、targetTempC
5. **LoadBatch 装窑批次** — kilnId、scheduleId、loadDate、pieceCount、glazeNotes、status（planned|loaded|fired|unloaded）
6. **Dashboard** — 窑炉数、firing 中窑数、本月装窑批次数、approved 制度数（本月按 loadDate 的 UTC 日历月）

## 主要 API

前缀均为 `/api`：

- `POST /api/auth/login`、`GET /api/auth/me`
- `GET|POST|PUT|DELETE /api/studios`
- `GET|POST|PUT|DELETE /api/kilns`
- `GET|POST|PUT|DELETE /api/firing-schedules`
- `GET|POST|PUT|DELETE /api/load-batches`
- `GET /api/dashboard`

前端经 Nginx 将 `/api` 反代到后端 `http://backend:8700`。

## 端口映射

| 服务 | 宿主机 | 容器 |
|------|--------|------|
| Frontend | 3700 | 80 |
| Backend | 8700 | 8700 |
| Postgres | 5436 | 5432 |

## 目录结构

```
KilnLog-01/
├── docker-compose.yml
├── README.md
├── .gitignore
├── backend/          # .NET 8 Web API
└── frontend/         # React + Vite + nginx
```

## 界面配色

窑火橙（`#e85d04` / `#ffba08`）与炭灰（`#1a1614` / `#322b26`）主题。
