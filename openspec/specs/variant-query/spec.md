# variant-query Specification

## Purpose
TBD - created by archiving change shader-variant-collection-website. Update Purpose after archive.
## Requirements
### Requirement: Get all variant data via HTTP GET
系统 SHALL 提供 `GET /data` 端点，返回所有已存储的变体数据。

#### Scenario: Query all data
- **WHEN** 客户端发送 GET /data
- **THEN** 系统返回 JSON 数组，包含所有变体记录

#### Scenario: Response format
- **WHEN** 客户端发送 GET /data
- **THEN** 返回格式为 `[{ "message": "...", "time": "ISO时间" }, ...]`

### Requirement: Return data in reverse chronological order
系统 SHALL 按时间倒序返回数据，最新的数据在数组前面。

#### Scenario: Newest first
- **WHEN** 先提交 A，后提交 B
- **AND** 客户端查询 GET /data
- **THEN** 返回数组中 B 在 A 之前

### Requirement: Clear all data
系统 SHALL 提供 `POST /clear` 端点，清空所有已存储的变体数据。

#### Scenario: Clear data
- **WHEN** 客户端发送 POST /clear
- **THEN** 系统返回 `{ "ok": true }`，所有数据被清空

#### Scenario: Query after clear
- **WHEN** 执行 POST /clear 后
- **AND** 客户端发送 GET /data
- **THEN** 返回空数组 `[]`

