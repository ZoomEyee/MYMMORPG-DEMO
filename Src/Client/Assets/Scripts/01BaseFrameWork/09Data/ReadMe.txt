一.使用需要注意:
 1.所有管理器都只建议存储类对象;
 2.Xml管理器无法序列化字典,必须使用继承并实现了IXmlSerializable接口的类代替;
 3.ExcelDataCreater创建Xml数据时,需要将方法中的containerObj改成dynamic类型,
   使用dynamic需要将Unity的.Net API兼容级别切换为.Net 4.x(.Net Framework);
 4.Xml和Json管理器在反序列化List的时候会将集合视为可重复的,并将重复的元素添加到集合中,
   会导致集合中出现双倍内容,建议将List替换成HashSet这样的无重复元素的集合类型;
 5.二进制管理器可能无法反序列化继承了基类的自定义类,应该避免写这样的类作为成员;

二.如何使用表生成配置文件:
 1.表格配置规则:
  第一行:变量名
  第二行:变量类型(目前只支持int,float,bool,string)
  第三行:key,决定使用什么变量作为容器类的键
  第四行:描述内容,可随意填写
  第五行-第N行:具体数据信息
 2.使用流程:
  第一步:配制好Excel表,
  第二步:点击工具栏新增选项GameTool/GenerateExcelData/GenerateData生成类
  第三步:点击工具栏新增选项GameTool/GenerateExcelData/XX生成对应的配置文件
 3.更改生成路径:
  更改ExcelDataCreater和四个管理器中的路径属性