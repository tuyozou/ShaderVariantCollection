## ADDED Requirements

### Requirement: Client sends resource version with message
客户端在发送变体日志时 SHALL 同时发送资源版本字符串。

#### Scenario: Submit with version
- **WHEN** 客户端调用 Submit 方法发送变体日志
- **THEN** 请求体 SHALL 包含 `message` 和 `version` 两个字段

#### Scenario: Version from configuration
- **WHEN** 客户端实例配置了 `resourceVersion` 字段
- **THEN** Submit 调用 SHALL 自动使用配置的版本值

### Requirement: Server aggregates versions per message
服务端 SHALL 对同一 message 聚合所有出现过的版本到 versions 数组。

#### Scenario: New message with version
- **WHEN** 收到一个新的 message（不存在于数据库）
- **THEN** 服务端 SHALL 创建记录 `{ message, time, versions: [version] }`

#### Scenario: Existing message with new version
- **WHEN** 收到已存在的 message 但携带新的 version
- **THEN** 服务端 SHALL 将新 version 追加到该记录的 versions 数组
- **AND** 返回 `{ ok: true }`

#### Scenario: Existing message with existing version
- **WHEN** 收到已存在的 message 且 version 也已存在于 versions 数组
- **THEN** 服务端 SHALL 返回 `{ ok: false, exists: true }`
- **AND** 不修改数据

### Requirement: Versions are sorted semantically
versions 数组 SHALL 按语义化版本号排序。

#### Scenario: Semantic version ordering
- **WHEN** versions 数组包含 ["1.10.0", "1.2.0", "2.0.0"]
- **THEN** 排序结果 SHALL 为 ["1.2.0", "1.10.0", "2.0.0"]

#### Scenario: Non-semver fallback
- **WHEN** 版本号不符合 semver 格式
- **THEN** SHALL fallback 到字符串字典序排序

### Requirement: WebUI displays version tags
WebUI SHALL 在每条记录下方显示该记录关联的所有版本标签。

#### Scenario: Display versions under message
- **WHEN** 渲染一条包含 versions 数组的记录
- **THEN** SHALL 在 message 下方显示版本标签

#### Scenario: Collapse versions over threshold
- **WHEN** 一条记录的 versions 数量超过 10 个
- **THEN** 默认只显示前 10 个版本
- **AND** 显示 "+N 更多" 按钮

#### Scenario: Expand collapsed versions
- **WHEN** 用户点击 "+N 更多" 按钮
- **THEN** SHALL 展开显示所有版本
- **AND** 显示 "收起" 按钮

### Requirement: WebUI supports version filtering
WebUI SHALL 提供版本过滤功能，允许用户筛选特定版本的记录。

#### Scenario: Version dropdown populated from data
- **WHEN** WebUI 加载数据
- **THEN** SHALL 从所有记录中提取 unique 版本列表
- **AND** 填充到版本下拉选择器（按语义化版本排序）

#### Scenario: Filter by specific version
- **WHEN** 用户在版本下拉框选择特定版本
- **THEN** 列表 SHALL 只显示 versions 数组包含该版本的记录

#### Scenario: Clear version filter
- **WHEN** 用户选择 "所有版本" 选项
- **THEN** SHALL 显示所有记录（不按版本过滤）

#### Scenario: Combined search and version filter
- **WHEN** 用户同时输入搜索词和选择版本
- **THEN** SHALL 同时应用两个过滤条件（AND 关系）
