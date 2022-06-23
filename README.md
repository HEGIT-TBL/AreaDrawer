# NuGet packages
https://www.nuget.org/packages/HEGIT.AreaDrawer.RU/

https://www.nuget.org/packages/HEGIT.AreaDrawer.EN/
# Usage example
```
<Page
    x:Class="blank.Views.MainPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    xmlns:uc="clr-namespace:AreaDrawer;assembly=AreaDrawer"
    d:DesignHeight="450"
    d:DesignWidth="800"
    Style="{DynamicResource MahApps.Styles.Page}"
    mc:Ignorable="d">
    <Grid>
        <uc:AreaDrawer
            Width="{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Grid}}, Path=ActualWidth}"
            Height="{Binding RelativeSource={RelativeSource FindAncestor, AncestorType={x:Type Grid}}, Path=ActualHeight}"
            Stretch="UniformToFill" />
    </Grid>
</Page>
```
![1](https://user-images.githubusercontent.com/75380111/175225675-ff693123-675e-4869-a772-579b98e55c92.png)
![2](https://user-images.githubusercontent.com/75380111/175225679-9f5676bf-5053-4345-a3b8-50f87c1b64b7.png)
![3](https://user-images.githubusercontent.com/75380111/175225680-186faa91-143b-4673-9e8b-64043c35d22d.png)
