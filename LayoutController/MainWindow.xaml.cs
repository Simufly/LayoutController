using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace LayoutController
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        // 布局信息
        private List<LayoutInfo> layoutInfos = new List<LayoutInfo>();

        private bool isDragging;
        private Point lastPosition;
        private Canvas currentActiveRectangle = null;
        private TextBlock currentActiveTextBlock = null;

        Canvas screenRect;

        LayoutInfo selectedLayout;
        double scaleRatio;

        public MainWindow()
        {
            InitializeComponent();

            // 读取配置文件
            LoadXmlConfig();

            LayoutListBox.ItemsSource = layoutInfos;

            // Add mouse event handlers
            // 绑定事件处理器
            LayoutCanvas.MouseDown += LayoutCanvas_MouseDown;
            LayoutCanvas.MouseMove += LayoutCanvas_MouseMove;
            LayoutCanvas.MouseWheel += LayoutCanvas_MouseWheel;
            LayoutCanvas.MouseUp += LayoutCanvas_MouseUp;
        }

        public void LoadXmlConfig()
        {
            // 加载xml文件
            XDocument xdoc = XDocument.Load("Config/layout.xml");

            // 获取所有布局
            IEnumerable<XElement> layouts = xdoc.Root.Elements("Layout");

            foreach (var layout in layouts)
            {
                string layoutName = layout.Attribute("name").Value;
                bool layoutEnable = Convert.ToBoolean(layout.Attribute("enable").Value);
                int screenCount = Convert.ToInt32(layout.Attribute("screencount").Value);
                var totalPixelSize = layout.Attribute("totalPixelSize").Value.Split(',').Select(float.Parse).ToList();
                var startPoint = layout.Attribute("startPoint").Value.Split(',').Select(float.Parse).ToList();


                Console.WriteLine($"Layout name: {layoutName}, Enabled: {layoutEnable}");

                LayoutInfo layoutInfo = new LayoutInfo();
                layoutInfo.name = layoutName;
                layoutInfo.enable = layoutEnable;
                layoutInfo.screenCount = screenCount;
                layoutInfo.totalWidth = totalPixelSize[0];
                layoutInfo.totalHeight = totalPixelSize[1];
                layoutInfo.startX = startPoint[0];
                layoutInfo.startY = startPoint[1];
                layoutInfo.moduleLayoutInfos = new List<ModuleLayoutInfo>();

                foreach(var m in layout.Elements("Module"))
                {
                    string moduleName = m.Attribute("name").Value;
                    bool moduleVisible = Convert.ToBoolean(m.Attribute("visible").Value);
                    float moduleWidth = Convert.ToSingle(m.Attribute("width").Value);
                    float moduleHeight = Convert.ToSingle(m.Attribute("height").Value);
                    float moduleleft = Convert.ToSingle(m.Attribute("left").Value);
                    float moduletop = Convert.ToSingle(m.Attribute("top").Value);

                    Console.WriteLine($"Module name: {moduleName}, Visible: {moduleVisible}");
                    ModuleLayoutInfo moduleLayoutInfo = new ModuleLayoutInfo();
                    moduleLayoutInfo.name = moduleName;
                    moduleLayoutInfo.visible = moduleVisible;
                    moduleLayoutInfo.width = moduleWidth;
                    moduleLayoutInfo.height = moduleHeight;
                    moduleLayoutInfo.left = moduleleft;
                    moduleLayoutInfo.top = moduletop;

                    layoutInfo.moduleLayoutInfos.Add(moduleLayoutInfo);
                }

                layoutInfos.Add(layoutInfo);
                
            }
        }

        public void DrawLayout(LayoutInfo selectedLayout)
        {
            // 先清空Canvas
            LayoutCanvas.Children.Clear();

            scaleRatio = LayoutCanvas.ActualWidth * 0.8 / selectedLayout.totalWidth;

            // 我们将屏幕视为一个大矩形，所有模块在此大矩形内部进行绘制
            screenRect = new Canvas();
            screenRect.Width = selectedLayout.totalWidth * scaleRatio;
            screenRect.Height = selectedLayout.totalHeight * scaleRatio;
            //screenRect.Stroke = Brushes.LightBlue; // 边框颜色
            //screenRect.StrokeThickness = 2; // 边框宽度
            screenRect.Background = Brushes.LightBlue;

            // 添加屏幕矩形到Canvas
            LayoutCanvas.Children.Add(screenRect);
            Canvas.SetLeft(screenRect, (LayoutCanvas.ActualWidth - screenRect.Width) / 2);
            Canvas.SetTop(screenRect, (LayoutCanvas.ActualHeight - screenRect.Height) / 2);

            // 循环绘制模块矩形
            foreach (var module in selectedLayout.moduleLayoutInfos)
            {


                Canvas moduleRect = new Canvas();
                moduleRect.Width = module.width * scaleRatio;
                moduleRect.Height = module.height * scaleRatio;
                //moduleRect.Fill = Brushes.Green; // 填充颜色
                moduleRect.Background = Brushes.Green;
                //moduleRect.Stroke = Brushes.Black; // 边框颜色
                //moduleRect.StrokeThickness = 1; // 边框宽度

                moduleRect.Tag = module;

                // 添加模块矩形到Canvas
                LayoutCanvas.Children.Add(moduleRect);
                Canvas.SetLeft(moduleRect, (module.left) * scaleRatio + Canvas.GetLeft(screenRect));
                Canvas.SetTop(moduleRect, (module.top) * scaleRatio + Canvas.GetTop(screenRect));

                // 绘制模块名称
                TextBlock moduleName = new TextBlock();
                moduleName.Text = module.name;
                moduleName.Foreground = Brushes.White; // 字体颜色
                moduleName.FontSize = 12; // 字体大小

                // 添加模块名称到Canvas
                moduleRect.Children.Add(moduleName);

                if (module.visible)
                {
                    moduleRect.Visibility = Visibility.Visible;
                }
                else
                {
                    moduleRect.Visibility = Visibility.Collapsed;
                }


                // 更新名称的位置，使其在矩形内部居中
                //moduleName.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity)); // 测量获取真实宽高
                //double nameLeft = Canvas.GetLeft(moduleRect) + (moduleRect.Width - moduleName.DesiredSize.Width) / 2;
                //double nameTop = Canvas.GetTop(moduleRect) + (moduleRect.Height - moduleName.DesiredSize.Height) / 2;

                //Canvas.SetLeft(moduleName, nameLeft);
                //Canvas.SetTop(moduleName, nameTop);

                //moduleRect.Loaded += (s, e) =>
                //{
                //    // Position the TextBlock in the center of the Rectangle
                //    Canvas.SetLeft(moduleName, Canvas.GetLeft(moduleRect) + moduleRect.Width / 2 - moduleName.ActualWidth / 2);
                //    Canvas.SetTop(moduleName, Canvas.GetTop(moduleRect) + moduleRect.Height / 2 - moduleName.ActualHeight / 2);
                //};



                // 在创建Rectangle和TextBlock后添加
                //moduleRect.Tag = moduleName;
                moduleName.IsHitTestVisible = false;
                moduleRect.MouseDown += ModuleRectangle_MouseDown;


            }
        }


        private void LayoutListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 获取选中的布局信息
            selectedLayout = (LayoutInfo)LayoutListBox.SelectedItem;

            // 绘制布局
            DrawLayout(selectedLayout);
        }


        #region canvas 控制函数
        private void LayoutCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentActiveRectangle != null)
            {
                currentActiveRectangle.Background = Brushes.Green;
                currentActiveRectangle = null;
                //currentActiveTextBlock = null;
            }
        }

        private void ModuleRectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (currentActiveRectangle != null)
            {
                currentActiveRectangle.Background = Brushes.Green;
            }

            Canvas rectangle = (Canvas)sender;
            rectangle.Background = Brushes.Blue;

            currentActiveRectangle = rectangle;
            //currentActiveTextBlock = (TextBlock)currentActiveRectangle.Tag;
            lastPosition = e.GetPosition(LayoutCanvas);
            isDragging = true;

            e.Handled = true;
        }

        private void LayoutCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && currentActiveRectangle != null)
            {
                Point currentPosition = e.GetPosition(LayoutCanvas);

                double deltaX = currentPosition.X - lastPosition.X;
                double deltaY = currentPosition.Y - lastPosition.Y;

                double newLeft = Canvas.GetLeft(currentActiveRectangle) + deltaX;
                double newTop = Canvas.GetTop(currentActiveRectangle) + deltaY;

                // Prevent the rectangle from being dragged out of the canvas
                newLeft = Math.Max(newLeft, 0);
                newTop = Math.Max(newTop, 0);
                newLeft = Math.Min(newLeft, LayoutCanvas.ActualWidth - currentActiveRectangle.Width);
                newTop = Math.Min(newTop, LayoutCanvas.ActualHeight - currentActiveRectangle.Height);

                // 检查新的位置是否会使矩形超出屏幕
                //////////////////////////////////

                Canvas.SetLeft(currentActiveRectangle, newLeft);
                Canvas.SetTop(currentActiveRectangle, newTop);

                // Update the TextBlock position
                //Canvas.SetLeft(currentActiveTextBlock, newLeft + currentActiveRectangle.Width / 2 - currentActiveTextBlock.ActualWidth / 2);
                //Canvas.SetTop(currentActiveTextBlock, newTop + currentActiveRectangle.Height / 2 - currentActiveTextBlock.ActualHeight / 2);

                lastPosition = currentPosition;
            }
        }

        private void LayoutCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (currentActiveRectangle != null)
            {
                double scale = e.Delta > 0 ? 1.1 : 0.9;

                currentActiveRectangle.Width = Math.Max(currentActiveRectangle.Width * scale, 100);
                currentActiveRectangle.Height = Math.Max(currentActiveRectangle.Height * scale, 100);

                //Canvas.SetTop(currentActiveTextBlock, Canvas.GetTop(currentActiveRectangle) + currentActiveRectangle.Height / 2);
            }
        }

        private void LayoutCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
        }

        #endregion

        public void SaveLayout()
        {
            // 加载xml文件
            XDocument xdoc = XDocument.Load("Config/layout.xml");

            // 获取当前布局元素
            XElement layoutElement = xdoc.Root.Elements("Layout")
                .Where(l => l.Attribute("name").Value == selectedLayout.name)
                .First();

            // 清除旧的模块元素
            layoutElement.Elements("Module").Remove();

            // 遍历所有模块
            foreach (UIElement element in LayoutCanvas.Children)
            {
                if (element is Canvas rectangle && rectangle != screenRect)
                {
                    // 获取模块信息
                    ModuleLayoutInfo moduleInfo = (ModuleLayoutInfo)rectangle.Tag;

                    // 创建新的模块元素
                    XElement moduleElement = new XElement("Module");
                    moduleElement.SetAttributeValue("name", moduleInfo.name);
                    moduleElement.SetAttributeValue("visible", moduleInfo.visible.ToString());
                    moduleElement.SetAttributeValue("width", (rectangle.Width / scaleRatio).ToString());
                    moduleElement.SetAttributeValue("height", (rectangle.Height / scaleRatio).ToString());
                    moduleElement.SetAttributeValue("left", ((Canvas.GetLeft(rectangle) - Canvas.GetLeft(screenRect)) / scaleRatio).ToString());
                    moduleElement.SetAttributeValue("top", ((Canvas.GetTop(rectangle) - Canvas.GetTop(screenRect)) / scaleRatio).ToString());

                    // 添加到布局元素
                    layoutElement.Add(moduleElement);
                }
            }

            // 保存xml文件
            xdoc.Save("Config/layout.xml");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveLayout();
        }
    }
}
