﻿using UBViews.Helpers;
using UBViews.Services;
using UBViews.ViewModels;

namespace UBViews;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
		vm.contentPage = this;
	}
}

