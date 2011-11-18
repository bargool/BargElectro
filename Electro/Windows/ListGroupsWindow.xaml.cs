/*
 * User: Bargool
 * Date: 03.11.2011
 * Time: 14:28
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace BargElectro.Windows
{
	/// <summary>
	/// Interaction logic for ListGroupsWindow.xaml
	/// </summary>
	public partial class ListGroupsWindow : Window
	{
		List<string> items = new List<string>();
		public ListGroupsWindow()
			: this(new List<string>()) { }
		
		public ListGroupsWindow(List<string> items)
			:this(items, true) { }
		
		public ListGroupsWindow(List<string> items, bool CanAddNew)
		{
			this.items = items;
			InitializeComponent();
		}
		
		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Binding binding1 = new Binding();
			binding1.Source = items;
			lstGroups.SetBinding(ListBox.ItemsSourceProperty, binding1);
		}
	}
}