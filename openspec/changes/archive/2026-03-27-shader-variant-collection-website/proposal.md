## Why

需要一个工具来收集和查看游戏引擎运行时产生的 shader 变体信息。多个客户端会在运行过程中发送变体数据，需要集中存储、去重、并提供 Web 界面进行查看和搜索。这有助于分析 shader 变体的使用情况，优化 shader 编译和打包策略。

## What Changes

- 新建一个 Node.js + Express 服务器，监听端口 8880
- 提供 HTTP API 接收客户端提交的 shader 变体数据
- 使用 JSON 文件存储数据，内存 HashSet 实现去重
- 提供 Web 界面展示数据，支持刷新、清空、实时搜索、分页功能

## Capabilities

### New Capabilities

- `variant-submission`: 接收客户端提交的变体数据，去重后持久化存储
- `variant-query`: 提供 HTTP 接口获取全部变体数据
- `web-interface`: Web 界面展示变体列表，支持刷新、清空、实时搜索、分页

### Modified Capabilities

(无，这是新项目)

## Impact

- 新增文件：`server.js`, `public/index.html`, `data.json`, `package.json`
- 依赖：Node.js 运行时，Express 框架
- 端口占用：8880
- 存储：本地 JSON 文件
