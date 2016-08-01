# PullToRefresh.UWP
Generic Pull Down to Refresh implementation for UWP.

中文说明参见 [http://www.cnblogs.com/ms-uap/p/4814507.html](http://www.cnblogs.com/ms-uap/p/4814507.html)。

![pre1](http://images2015.cnblogs.com/blog/700062/201509/700062-20150924182012084-146770665.gif)


## Installation
Preferred way to install this library is to use NuGet:

```
PM> Install-Package PullToRefresh.UWP
```
or
```
nuget install PullToRefresh.UWP
```

The NuGet link is [https://www.nuget.org/packages/PullToRefresh.UWP](https://www.nuget.org/packages/PullToRefresh.UWP)

Or you may need to refer the library project in your VS solution, or copy all related compiled files.


## Getting Started
After referring the package, you can add namespace reference in your XAML file:
```xaml
<Page x:Class="PullToRefresh.UWP.Sample.Scenarios.ListViewInside"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:local="using:PullToRefresh.UWP.Sample.Scenarios"
      xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
      xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
      xmlns:pr="using:PullToRefresh.UWP"
      mc:Ignorable="d">
```

Then put any element as `PullToRefreshBox`'s content and hook `RefreshInvoked` event:
```xaml
<pr:PullToRefreshBox x:Name="pr" RefreshInvoked="PullToRefreshBox_RefreshInvoked">
    <ListView x:Name="lv" ItemTemplate="{StaticResource ColorfulRectangle}" />
</pr:PullToRefreshBox>
```
The handler type is `TypedEventHandler<DependencyObject, object>`. The first parameter `sender` refers to `PullToRefreshBox.Content`, and the second parameter is reserved as `null`.

Then you just get PullToRefresh function.


## More usage
1. Common properties:
* `double RefreshThreshold {get;set;}`: Get/set the threshold to trigger a refresh.
* `DataTemplate TopIndicatorTemplate {get;set;}`: Customize top indicator. The `DataContext` is a `double` value meaning the ratio of pulled distance to threshold. May be greater than 100%. The top indicator template decides max distance to pull down.

2. And this package provide a simple `PullRefreshProgressControl` to enable customized indicator.

For users who are not Simp.Chinese speaker, `PullRefreshProgressControl` has two property to customize native language as indicator text, you can set them like this:
```xaml
<pr:PullToRefreshBox x:Name="pr" RefreshInvoked="PullToRefreshBox_RefreshInvoked">
    <pr:PullToRefreshBox.TopIndicatorTemplate>
        <DataTemplate>
            <pr:PullRefreshProgressControl Progress="{Binding}"
                                           PullToRefreshText="Pull"
                                           ReleaseToRefreshText="Release" />
        </DataTemplate>
    </pr:PullToRefreshBox.TopIndicatorTemplate>

    <ListView x:Name="lv" ItemTemplate="{StaticResource ColorfulRectangle}" />
</pr:PullToRefreshBox>
```
Don't forget to bind `Progress`.

3. `PullRefreshProgressControl` has two `VisualState`s:
* `Normal`
* `ReleaseToRefresh`

which make richer customization possible:
![pre2](http://images2015.cnblogs.com/blog/700062/201509/700062-20150916195935617-990341701.gif)

```xaml
<Grid>
    <pr:PullToRefreshBox RefreshInvoked="pr_RefreshInvoked">
        <pr:PullToRefreshBox.TopIndicatorTemplate>
            <DataTemplate>
                <Grid Background="LightBlue"
                      Height="130"
                      Width="200">
                    <pr:PullRefreshProgressControl Progress="{Binding}"
                                                   HorizontalAlignment="Center"
                                                   VerticalAlignment="Bottom">
                        <pr:PullRefreshProgressControl.Template>
                            <ControlTemplate>
                                <Grid>
                                    <VisualStateManager.VisualStateGroups>
                                        <VisualStateGroup x:Name="VisualStateGroup">
                                            <VisualState x:Name="Normal" />
                                            <VisualState x:Name="ReleaseToRefresh">
                                                <Storyboard>
                                                    <ObjectAnimationUsingKeyFrames Storyboard.TargetName="txt" Storyboard.TargetProperty="Text">
                                                        <DiscreteObjectKeyFrame KeyTime="0" Value="释放刷新" />
                                                    </ObjectAnimationUsingKeyFrames>
                                                </Storyboard>
                                            </VisualState>
                                        </VisualStateGroup>
                                    </VisualStateManager.VisualStateGroups>

                                    <Grid.RowDefinitions>
                                        <RowDefinition Height="auto" />
                                        <RowDefinition Height="auto" />
                                    </Grid.RowDefinitions>

                                    <TextBlock x:Name="txt"
                                               Text="下拉刷新"
                                               Grid.Row="1"
                                               FontSize="20"
                                               HorizontalAlignment="Center" />
                                    <TextBlock Text="{Binding}"
                                               FontSize="24"
                                               Foreground="Gray"
                                               HorizontalAlignment="Center" />

                                </Grid>
                            </ControlTemplate>
                        </pr:PullRefreshProgressControl.Template>
                    </pr:PullRefreshProgressControl>

                </Grid>

            </DataTemplate>
        </pr:PullToRefreshBox.TopIndicatorTemplate>

        <ListView x:Name="ic">
            <ListView.ItemContainerStyle>
                <Style TargetType="ListViewItem">
                    <Setter Property="HorizontalAlignment" Value="Center" />
                </Style>
            </ListView.ItemContainerStyle>
            <ListView.ItemTemplate>
                <DataTemplate>
                    <Rectangle Width="100" Height="200">
                        <Rectangle.Fill>
                            <SolidColorBrush Color="{Binding}" />
                        </Rectangle.Fill>
                    </Rectangle>
                </DataTemplate>
            </ListView.ItemTemplate>

        </ListView>
    </pr:PullToRefreshBox>
</Grid>
```

4. For more complex scenarios, try to use your own top template and bind to `DataContext`.

## Have fun coding UWP :)