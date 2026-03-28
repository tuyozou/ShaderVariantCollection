## ADDED Requirements

### Requirement: Display variant list
Web 界面 SHALL 展示变体数据列表，每条记录显示完整 message 和时间。

#### Scenario: Show full message
- **WHEN** 用户访问 Web 界面
- **THEN** 每条变体记录完整显示 message 内容和提交时间

#### Scenario: Newest on top
- **WHEN** 用户查看列表
- **THEN** 最新的数据显示在最上方

### Requirement: Pagination
Web 界面 SHALL 支持分页，每页显示 50 条数据。

#### Scenario: Page size
- **WHEN** 数据超过 50 条
- **THEN** 首页显示最新的 50 条，提供分页导航

#### Scenario: Navigate pages
- **WHEN** 用户点击第 2 页
- **THEN** 显示第 51-100 条数据

### Requirement: Refresh button
Web 界面 SHALL 提供刷新按钮，点击后重新加载数据。

#### Scenario: Refresh data
- **WHEN** 用户点击刷新按钮
- **THEN** 从服务器获取最新数据并刷新列表

### Requirement: Clear button
Web 界面 SHALL 提供清空按钮，点击后清空所有数据（无需二次确认）。

#### Scenario: Clear all data
- **WHEN** 用户点击清空按钮
- **THEN** 调用 POST /clear 接口，清空数据后刷新列表

### Requirement: Real-time search
Web 界面 SHALL 提供搜索框，支持实时过滤数据。

#### Scenario: Filter by keyword
- **WHEN** 用户在搜索框输入 "_AAA"
- **THEN** 列表只显示 message 中包含 "_AAA" 的记录

#### Scenario: Search with debounce
- **WHEN** 用户连续输入字符
- **THEN** 搜索在停止输入约 200ms 后触发，避免频繁过滤

#### Scenario: Clear search
- **WHEN** 用户清空搜索框
- **THEN** 显示所有数据

### Requirement: Display statistics
Web 界面 SHALL 显示当前数据统计信息。

#### Scenario: Show total count
- **WHEN** 用户查看界面
- **THEN** 显示 "显示 X-Y / 共 Z 条" 格式的统计信息
