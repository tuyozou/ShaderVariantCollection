## ADDED Requirements

### Requirement: Submit variant data via HTTP POST
系统 SHALL 提供 `POST /submit` 端点，接收客户端提交的变体数据。

#### Scenario: Submit new variant
- **WHEN** 客户端发送 POST /submit，body 为 `{ "message": "shader_AAA" }`
- **THEN** 系统返回 `{ "ok": true }`，并将数据存储

#### Scenario: Submit with large payload
- **WHEN** 客户端发送 POST /submit，message 内容为 1KB 或更大
- **THEN** 系统正常接收并存储

### Requirement: Deduplicate variant data
系统 SHALL 对变体数据进行去重处理，相同的 message 只存储一次。

#### Scenario: Submit duplicate variant
- **WHEN** 客户端 A 发送 `{ "message": "shader_AAA" }`
- **AND** 客户端 B 再次发送 `{ "message": "shader_AAA" }`
- **THEN** 系统返回 `{ "ok": false, "exists": true }`，不重复存储

#### Scenario: Submit similar but different variants
- **WHEN** 客户端发送 `{ "message": "shader_AAA" }`
- **AND** 客户端发送 `{ "message": "shader_AAA_v2" }`
- **THEN** 两条数据都被存储（字符串不同）

### Requirement: Record submission time
系统 SHALL 在存储变体数据时记录服务器接收时间。

#### Scenario: Timestamp is server time
- **WHEN** 客户端发送变体数据
- **THEN** 存储的 `time` 字段为服务器当前时间（ISO 格式）

### Requirement: Persist data immediately
系统 SHALL 在接收到新数据后立即持久化到 JSON 文件。

#### Scenario: Immediate persistence
- **WHEN** 客户端发送新的变体数据
- **THEN** 数据立即写入 data.json 文件，服务重启后数据不丢失
