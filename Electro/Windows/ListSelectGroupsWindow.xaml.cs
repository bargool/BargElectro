/*
 * Created by SharpDevelop.
 * User: aleksey
 * Date: 19.11.2011
 * Time: 16:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
	/// Interaction logic for ListSelectGroupsWindow.xaml
	/// </summary>
	public partial class ListSelectGroupsWindow : Window
	{
		List<string> items = new List<string>();
		string group;
		public string Group {
			get{return group;}
		}
		public ListSelectGroupsWindow()
			: this(new List<string>()) { }
		
		public ListSelectGroupsWindow(List<string> items)
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
		
		void BtnChoose_Click(object sender, RoutedEventArgs e)
		{
			if (lstGroups.SelectedIndex==-1)
			{
				MessageBox.Show("Необходимо выбрать группу!");
			}
			else
			{
				group = lstGroups.SelectedValue as string;
				DialogResult = group.Length!=0;
			}
		}
	}
}