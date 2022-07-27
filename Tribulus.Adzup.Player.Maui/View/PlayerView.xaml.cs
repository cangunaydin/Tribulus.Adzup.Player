using Tribulus.Adzup.Player.Maui.Controls;
using Tribulus.Adzup.Player.Maui.ViewModel;

namespace Tribulus.Adzup.Player.Maui.View;

public partial class PlayerView : ContentPage
{
	public PlayerView(PlayerViewModel playerViewModel)
	{
		InitializeComponent();
		this.BindingContext = playerViewModel;
    }
}