using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Z;

namespace ZGrid.Demo.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private object? selectedObject;

    public MainWindowViewModel()
    {
        var settings = new ZGrid.Demo.Models.MySettings();

        var cascaderData = new List<CascaderNode>
        {
            new CascaderNode("һ�� A", new List<CascaderNode>
            {
                new CascaderNode("���� A1", new List<CascaderNode>
                {
                    new CascaderNode("���� A1-1"),
                    new CascaderNode("���� A1-2"),
                    new CascaderNode("���� A1-3"),
                }),
                new CascaderNode("���� A2", new List<CascaderNode>
                {
                    new CascaderNode("���� A2-1"),
                    new CascaderNode("���� A2-2"),
                })
            }),
            new CascaderNode("һ�� B", new List<CascaderNode>
            {
                new CascaderNode("���� B1", new List<CascaderNode>
                {
                    new CascaderNode("���� B1-1"),
                    new CascaderNode("���� B1-2"),
                }),
                new CascaderNode("���� B2", new List<CascaderNode>
                {
                    new CascaderNode("���� B2-1"),
                })
            }),
            new CascaderNode("һ�� C", new List<CascaderNode>
            {
                new CascaderNode("���� C1", new List<CascaderNode>
                {
                    new CascaderNode("���� C1-1"),
                    new CascaderNode("���� C1-2"),
                    new CascaderNode("���� C1-3"),
                    new CascaderNode("���� C1-4"),
                })
            })
        };
        settings.CascaderSource.AddRange(cascaderData);

        SelectedObject = settings;
    }
}
