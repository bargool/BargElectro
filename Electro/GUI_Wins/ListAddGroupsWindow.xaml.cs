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
	public partial class ListAddGroupsWindow : Window
	{
		List<string> items = new List<string>();
		string group;
		public string Group {
			get{return group;}
		}
		public ListAddGroupsWindow()
			: this(new List<string>()) { }
		
		public ListAddGroupsWindow(List<string> items)
		{
			this.items = items;
			InitializeComponent();
		}
		
		void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Binding binding1 = new Binding();
			binding1.Source = items;
			lstGroups.SetBinding(ListBox.ItemsSourceProperty, binding1);
			grdChoose.IsEnabled = items.Count!=0;
		}
		
		void BtnAdd_Click(object sender, RoutedEventArgs e)
		{
			if (txtNewName.Text.Length==0)
			{
				MessageBox.Show("Невозможно добавить группу с пустым именем!");
			}
			else
			{
				group = txtNewName.Text;
				DialogResult = group.Length!=0;
			}
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