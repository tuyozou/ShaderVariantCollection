## 1. C# 客户端修改

- [x] 1.1 在 SubmitRequest 类中添加 version 字段
- [x] 1.2 在 ShaderVariantClient 类中添加 resourceVersion 配置字段（带 Header 和 Tooltip）
- [x] 1.3 修改 SubmitCoroutine 方法，将 resourceVersion 添加到请求体

## 2. 服务端修改

- [x] 2.1 添加 semver 排序函数（支持 fallback 到字符串排序）
- [x] 2.2 修改数据结构，支持 versions 数组
- [x] 2.3 修改 messageSet 为 Map，存储 message -> versions 映射
- [x] 2.4 修改 POST /submit 处理逻辑：新 message 创建 versions 数组，已存在则聚合版本
- [x] 2.5 修改 loadData 函数，重建 message -> versions 映射
- [x] 2.6 清空现有 data.json 数据

## 3. WebUI 修改

- [x] 3.1 添加版本标签的 CSS 样式
- [x] 3.2 添加版本下拉选择器的 HTML 和 CSS
- [x] 3.3 实现 extractAllVersions 函数，从数据中提取并排序所有版本
- [x] 3.4 实现版本下拉选择器的渲染和事件处理
- [x] 3.5 修改 applyFilter 函数，支持版本过滤（AND 关系）
- [x] 3.6 实现版本标签渲染（含折叠/展开逻辑，阈值 10 个）
- [x] 3.7 修改 render 函数，在每条记录中显示版本标签

## 4. 验证

- [x] 4.1 手动测试：发送不同版本的相同 message，验证聚合逻辑
- [x] 4.2 手动测试：WebUI 版本过滤和搜索组合
- [x] 4.3 手动测试：版本标签折叠/展开
