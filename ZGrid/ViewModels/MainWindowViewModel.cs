using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ZGrid.Models;

namespace ZGrid.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? selectedObject;

        public MainWindowViewModel()
        {
            var settings = new MySettings();

            // Provide cascader data source here (external to models)
            var cascaderData = new List<CascaderNode>
            {
                new CascaderNode("一级 A", new List<CascaderNode>
                {
                    new CascaderNode("二级 A1", new List<CascaderNode>
                    {
                        new CascaderNode("三级 A1-1"),
                        new CascaderNode("三级 A1-2"),
                        new CascaderNode("三级 A1-3"),
                    }),
                    new CascaderNode("二级 A2", new List<CascaderNode>
                    {
                        new CascaderNode("三级 A2-1"),
                        new CascaderNode("三级 A2-2"),
                    })
                }),
                new CascaderNode("一级 B", new List<CascaderNode>
                {
                    new CascaderNode("二级 B1", new List<CascaderNode>
                    {
                        new CascaderNode("三级 B1-1"),
                        new CascaderNode("三级 B1-2"),
                    }),
                    new CascaderNode("二级 B2", new List<CascaderNode>
                    {
                        new CascaderNode("三级 B2-1"),
                    })
                }),
                new CascaderNode("一级 C", new List<CascaderNode>
                {
                    new CascaderNode("二级 C1", new List<CascaderNode>
                    {
                        new CascaderNode("三级 C1-1"),
                        new CascaderNode("三级 C1-2"),
                        new CascaderNode("三级 C1-3"),
                        new CascaderNode("三级 C1-4"),
                    })
                })
            };
            settings.CascaderSource.AddRange(cascaderData);

            SelectedObject = settings;
        }
    }
}
