namespace PluginExample
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for TextTreeControl.
    /// </summary>
    public partial class TextTreeControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _fileText { get; set; }
        public string fileText {
            get { return _fileText; }
            set
            {
                if (value != _fileText)
                {
                    _fileText = value;
                    OnPropertyChanged("fileText");
                    sentencesTree = Parse(_fileText);
                    AttachTreeFromNodes(FileNode, sentencesTree);
                }
                if (!String.IsNullOrEmpty(_fileText) && !textTreeGrid.IsVisible)
                {
                    textTreeGrid.Visibility = Visibility.Visible;
                }
            }
        }

        public Node sentencesTree { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextTreeControl"/> class.
        /// </summary>
        public TextTreeControl()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                fileText = File.ReadAllText(openFileDialog.FileName);
            }
        }

        private Node Parse(string text)
        {
            int startIndex = 0;
            return Parse(text, ref startIndex, true);
        }

        private Node Parse(string text, ref int index, bool parentNode = false)
        {
            var result = new Node();
            var builder = new StringBuilder();

            var writeStarted = false;
            var firstLevelChild = false;

            for (int i = index; i < text.Length; i++, index++)
            {
                if (!writeStarted && !parentNode)
                {
                    if (!(text[i].CompareTo(' ') == 0))
                    {
                        builder.Append(text[i]);
                        writeStarted = true;
                        firstLevelChild = Char.IsUpper(text[i]);
                    }
                } else {
                    if (parentNode && text[i].CompareTo(' ') == 0)
                    {
                        continue;
                    }
                    if (text[i].CompareTo('(') == 0 || (parentNode && Char.IsUpper(text[i])))
                    {
                        var inner = Parse(text, ref i);
                        result.children.Add(inner);
                        builder.Append(inner.text);
                        index = i;
                    } else if (text[i].CompareTo(')') == 0 || (firstLevelChild && text[i].CompareTo('.') == 0)) {
                        builder.Append(text[i]);
                        result.text = builder.ToString();
                        return result;
                    } else {
                        builder.Append(text[i]);
                    }
                }
            }

            result.text = builder.ToString();
            return result;
        }

        private void AttachTreeFromNodes(TreeViewItem baseItem, Node node)
        {
            TreeViewItem ParentItem = MapNodeToTreeViewItem(node);
            baseItem.Items.Add(ParentItem);
        }

        private TreeViewItem MapNodeToTreeViewItem(Node node)
        {
            var result = new TreeViewItem();
            result.Header = node.text;
            foreach (var child in node.children)
            {
                var tempChildItem = MapNodeToTreeViewItem(child);
                result.Items.Add(tempChildItem);
            }
            return result;
        }
    }

    public class Node { 
        public string text { get; set; }
        public List<Node> children { get; set; } = new List<Node>();
    }
}