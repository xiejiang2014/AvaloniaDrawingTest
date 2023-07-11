# AvaloniaDrawingTest
 Comparing the drawing performance of Avalonia and WPF


Test code from:
https://github.com/AvaloniaUI/Avalonia/blob/master/samples/ControlCatalog/Pages/CustomDrawing.xaml.cs

I modified it into a separate Avalonia application.
At the same time I also translated it to the WPF version.
The test tries to draw 10k circles each frame.

On the windows platform, you can run WpfDrawingTest.exe and AvaloniaDrawingTest.Desktop.exe after compilation to compare the performance gap between the two UI frameworks.