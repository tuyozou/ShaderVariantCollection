## Why

客户端发送的 shader 变体日志目前只包含 message，无法追踪问题出现在哪些资源版本中。需要新增资源版本字段，并在 WebUI 中聚合显示每条日志关联的所有版本，便于定位问题影响范围。

## What Changes

- C# 客户端新增 `resourceVersion` 配置字段，Submit 时自动携带版本
- 服务端接收 `{ message, version }`，message 去重但聚合 versions 数组
- 服务端 versions 按语义化版本排序存储
- WebUI 新增版本下拉过滤器，支持按版本筛选
- WebUI 每条记录显示版本标签，默认显示 10 个，超出折叠展示
- 清空现有 data.json 数据（不兼容老数据结构）

## Capabilities

### New Capabilities
- `version-tracking`: 资源版本追踪能力，包含客户端发送版本、服务端聚合版本、WebUI 展示和过滤版本

### Modified Capabilities
<!-- 无需修改现有能力的需求规格 -->

## Impact

- **C# 客户端**: `ShaderVariantClient.cs` - 新增字段和修改请求结构
- **服务端**: `server.js` - 修改存储结构和提交逻辑
- **WebUI**: `public/index.html` - 新增版本过滤器和版本标签展示
- **数据**: `data.json` - 结构变更，需清空重来
