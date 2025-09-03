# ZGrid

һ������ .NET 9 �� Avalonia �Ŀ�ƽ̨����ʾ��Ӧ�ã�����չʾһ���򵥶�ʵ�õ� PropertyGrid��������壩�ؼ����ÿؼ������������ʾ�������ԣ��ṩ�����༭���������������� MVVM �ṹ�������������Ŀ�и��á�

- ֧��ƽ̨��Windows / macOS / Linux
- ����ջ��.NET 9��Avalonia 11.2��CommunityToolkit.Mvvm

## ��������
- MVVM ���ȣ��޽����߼��ѵ��� code-behind
- �������ԣ�Attribute���ķ�����Ԫ����֧�֣�
  - [Category]��[DisplayName]��[Description]
- ���û����༭������ǰ����
  - Text���ַ����������ǳ�������/ö�ٵ����ͣ����ַ�����ʽ�༭��
  - Bool����ѡ��
  - Enum������ѡ��
- ����۵�/չ����ָ������
- ѡ���������ײ���˵����塱���Զ�չʾ��Ӧ���Ե�����
- ���ڶ��Ƶ���ʽ��ģ�壨Fluent ���

## ��ͼ

![](./ZGrid/Assets/2025-09-03_142307_650.png)

## ���ٿ�ʼ
### ǰ������
- .NET SDK 9.0+

### ����������
- ��ԭ������`dotnet restore`
- ������`dotnet build`
- ���У�`dotnet run --project ZGrid/ZGrid.csproj`

## ��Ŀ�ṹ
- ZGrid/
  - Controls/
    - PropertyGrid.axaml, PropertyGrid.axaml.cs ���� �ɸ��õ��������ؼ�
  - Models/
    - MySettings.cs ���� ʾ������ģ�ͣ���ע�⣩
    - PropertyGridModels.cs ���� �ؼ�����ģ�͡�ת�������༭�����͵�
  - Views/
    - MainWindow.axaml ���� ���� PropertyGrid �Ĵ���
  - ViewModels/
    - MainWindowViewModel.cs ���� �ṩ SelectedObject ����Դ

## PropertyGrid �ؼ�˵��
PropertyGrid �����в�����ʾ���ԣ����Ϊ���ƣ�DisplayName�����Ҳ�Ϊ�༭������ Category ���з��顣

### �ؼ����ԣ����� API��
- SelectedObject (object?)��Ҫչʾ�ͱ༭�Ķ���
- SelectedEntry (PropertyEntry?)����ǰѡ�е�������Ŀ��֧�ְ󶨣�

### ����ģ��
- PropertyEntry����װ PropertyDescriptor����¶ DisplayName��Description��Category��EditorKind���Լ��༭ֵ��StringValue��BoolValue��EnumValue��
- CategoryGroup��Name��Items��IsExpanded
- ת������EditorKindEqualsConverter��NotConverter��BoolToGlyphConverter

### ֧�ֵı༭������ǰ��
- Text��TextBox �� StringValue
- Bool��CheckBox �� BoolValue
- Enum��ComboBox �� EnumValues/EnumValue

˵����Ϊ����ʾ����࣬���ڡ���ѡ�ȱ༭�����Ƴ���

### ʹ��ʾ��
XAML�����������ͼ�У���
```xml
<controls:PropertyGrid SelectedObject="{Binding SelectedObject}" />
```

ViewModel���ṩһ������ʵ������
```csharp
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? selectedObject = new MySettings();
}
```

ģ�ͣ����ע�⣩��
```csharp
public class MySettings
{
    [Category("General"), DisplayName("User Name"), Description("Shown across the app.")]
    public string? UserName { get; set; } = Environment.UserName;

    [Category("General"), DisplayName("Enable Feature"), Description("Toggle a flag.")]
    public bool EnableFeature { get; set; } = true;

    [Category("Advanced"), DisplayName("Log Level"), Description("Select log level.")]
    public LogLevel LogLevel { get; set; } = LogLevel.Info;
}
```

## ��������ʽ
- �۵���ť����ʽ�� `collapse-btn`
- �е���ͣ/ѡ�У��� PropertyGrid.axaml �� ListBoxItem ��ʽ�ж���
- �ɸ�����Ҫ�滻��ɫ���߿�ģ���Է�����Ĳ�Ʒ���

## �滮��Roadmap��
- ��ѡ�༭�������ڡ���ֵ����ѡ�ȣ��Ŀɲ����չ
- У�������չʾ
- ��������/����

## ���빱��
- Fork �ֿ�
- ���ڹ��ܽ�����֧
- ����ʹ���������ύ��Ϣ
- �ύ PR ʱ�븽�ϸĶ�˵�����漰 UI �ĸĶ��븽��ǰ���ͼ

�ϴ�Χ�ĸĶ����������� Issue �������ۡ�

## ���Э��
����Ŀʹ�� MIT ���֤��������������ɵ�ʹ�á����ơ��޸ġ��ϲ�������������ɼ�/�����۱�����ĸ��������������һͬ������Ȩ���������������μ� [LICENSE](LICENSE)��