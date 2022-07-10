using Tribulus.Adzup.Player.Maui.ViewModel;

namespace Tribulus.Adzup.Player.Maui.View;

public partial class PlayerView : ContentPage
{
	public PlayerView(PlayerViewModel playerViewModel)
	{
		InitializeComponent();
		this.BindingContext = playerViewModel;
    }

	private void skiaView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
	{

	}
}