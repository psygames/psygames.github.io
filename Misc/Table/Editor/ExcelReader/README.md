#### 表格配置规范：
- 第一行为字段名
- 第二行为字段类型
- 第三行为字段描述
- 第一列必须为id列
- 注意，当id字段名带有 ”(flip)” 后缀标识，代表表格行列翻转。适用于常量表，多语言表等。

#### 数据类型支持：

支持数据类型 | 类型示例 | 数据格式示例 |  说明  
-|-|-|-
全部基础类型 | float | 10.1 | 包括：bool, short, int, long, float, double, string
Unity数据类型 | Vector3 | (1.1,2,3) | 包括：Vector2, Vector3, Vector4, RangeInt
数组支持 | Vector2[] | [(1,2),(2,3)] | 全数据类型均支持数组格式
元组支持 | Tuple<int,float> | (2,2.5)  | 全数据类型均支持元组格式
字典支持 | Dictionary<string,float> | [("hp",100),("mp",50)] | 全数据类型均支持字典格式
