using Microsoft.Win32;
using System.Text;
using System.Text.RegularExpressions;

namespace RegisterLab3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FillTree();
            FillContextMenuStrip1();
            FillContextMenuStrip2();
        }

        private void FillTree()
        {
            try
            {
                
                TreeNode _rKeyNode1 = new TreeNode { Text = Registry.ClassesRoot.Name };
                FillTreeNode(_rKeyNode1, Registry.ClassesRoot);
                treeView1.Nodes.Add(_rKeyNode1);
                TreeNode _rKeyNode2 = new TreeNode { Text = Registry.CurrentUser.Name };
                FillTreeNode(_rKeyNode2, Registry.CurrentUser);
                treeView1.Nodes.Add(_rKeyNode2);
                TreeNode _rKeyNode3 = new TreeNode { Text = Registry.LocalMachine.Name };
                FillTreeNode(_rKeyNode3, Registry.LocalMachine);
                treeView1.Nodes.Add(_rKeyNode3);
                TreeNode _rKeyNode4 = new TreeNode { Text = Registry.Users.Name };
                FillTreeNode(_rKeyNode4, Registry.Users);
                treeView1.Nodes.Add(_rKeyNode4);
                TreeNode _rKeyNode5 = new TreeNode { Text = Registry.CurrentConfig.Name };
                FillTreeNode(_rKeyNode5, Registry.CurrentConfig);
                treeView1.Nodes.Add(_rKeyNode5);
            }
            catch (Exception ex) { }
        }
        private void FillTreeNode(TreeNode treeNode, RegistryKey sourceKey)
        {
            try
            {
                foreach (string rKey2 in sourceKey.GetSubKeyNames())
                {
                    RegistryKey sourceSubKey = sourceKey.OpenSubKey(rKey2, true);
                    TreeNode _rNode = new TreeNode();
                    _rNode.Text = sourceSubKey.Name.Remove(0, sourceSubKey.Name.LastIndexOf('\\') + 1); 
                    FillTreeNode(_rNode, sourceSubKey);
                    treeNode.Nodes.Add(_rNode);
                }
            }
            catch (Exception ex) { }
        }
        private void FillContextMenuStrip1()
        {
            ToolStripMenuItem createSubkey = new ToolStripMenuItem("Create subkey");
            ToolStripMenuItem createParam = new ToolStripMenuItem("Create parameter");
            ToolStripMenuItem changeSubkey = new ToolStripMenuItem("Change");
            ToolStripMenuItem deleteSubkey = new ToolStripMenuItem("Delete");
            contextMenuStrip1.Items.AddRange(new[] { createSubkey, createParam, changeSubkey, deleteSubkey });
            treeView1.ContextMenuStrip = contextMenuStrip1;
            createSubkey.Click += CreateSubkey_Click; ;
            createParam.Click += CreateParam_Click;
            changeSubkey.Click += ChangeSubkey_Click;
            deleteSubkey.Click += DeleteSubkey_Click;
        }

        private void ChangeSubkey_Click(object? sender, EventArgs e)
        {
            RegistryKey _rKey;
            int depth;
            string result = GetPathNode(treeView1.SelectedNode, out _rKey, out depth);

            if (textBox1.Text != string.Empty)
            {
                RegistryKey key = _rKey.OpenSubKey(result,true);

                if (key != null)
                {
                    //We need this part of code because
                    //in path we have full old path
                    //but in name we have to input only new name for update
                    //This part creates full path for new name
                    string path = CreateFullPath(result);

                    CopyKey(_rKey, result, path);
                    _rKey.DeleteSubKeyTree(result);
                }
                else
                {
                    MessageBox.Show("Incorrect path!");
                }
            }
            treeView1.Nodes.Clear();
            FillTree();
        }
        public bool CopyKey(RegistryKey parentKey, string keyNameToCopy, string newKeyName)
        {
            //Create new key
            RegistryKey destinationKey = parentKey.CreateSubKey(newKeyName, true);

            //Open the sourceKey we are copying from
            RegistryKey sourceKey = parentKey.OpenSubKey(keyNameToCopy, true);

            RecurseCopyKey(sourceKey, destinationKey);

            return true;
        }
        private void RecurseCopyKey(RegistryKey sourceKey, RegistryKey destinationKey)
        {
            //copy all the values
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object objValue = sourceKey.GetValue(valueName);
                RegistryValueKind valKind = sourceKey.GetValueKind(valueName);
                destinationKey.SetValue(valueName, objValue, valKind);
            }

            //For Each subKey 
            //Create a new subKey in destinationKey 
            //Call itself 
            foreach (string sourceSubKeyName in sourceKey.GetSubKeyNames())
            {
                RegistryKey sourceSubKey = sourceKey.OpenSubKey(sourceSubKeyName, true);
                RegistryKey destSubKey = destinationKey.CreateSubKey(sourceSubKeyName);
                RecurseCopyKey(sourceSubKey, destSubKey);
            }
        }
        private string CreateFullPath(string text)
        {
            string[] words = text.Split('\\');
            StringBuilder sb = new StringBuilder();
            int i = 1;
            foreach (var str in words)
            {
                if (i == words.Length)
                {
                    sb.Append(textBox1.Text);
                }
                else
                {
                    sb.Append($"{str}\\");
                }

                i++;
            }

            return sb.ToString();
        }
        private void DeleteSubkey_Click(object? sender, EventArgs e)
        {
            RegistryKey _rKey;
            int depth;
            string result = GetPathNode(treeView1.SelectedNode, out _rKey, out depth);
            RegistryKey rKey2 = _rKey.OpenSubKey(result,true);
            if (rKey2 != null)
            {
                _rKey.DeleteSubKeyTree(result);
                _rKey.Close();
            }
            else
            {
                MessageBox.Show("Incorrect subkey path!");
            }
            treeView1.Nodes.Clear();
            FillTree();
        }

        private void CreateParam_Click(object? sender, EventArgs e)
        {
            RegistryKey _rKey;
            int depth;
            string result = GetPathNode(treeView1.SelectedNode, out _rKey, out depth);
            RegistryKey rKey2 = _rKey.OpenSubKey(result,true);
            if (rKey2 != null)
            {
                if (textBox2.Text != null && textBox3.Text != null)
                {
                    rKey2.SetValue(textBox2.Text, textBox3.Text);
                    rKey2.Close();
                }
                else
                {
                    MessageBox.Show("Empty value for name and(or) value!");
                }
            }
            else
            {
                MessageBox.Show("Incorrect path!");
            }
            treeView1.Nodes.Clear();
            FillTree();
        }

        private void CreateSubkey_Click(object? sender, EventArgs e)
        {
            
            RegistryKey _rKey;
            int depth;
            string result = GetPathNode(treeView1.SelectedNode, out _rKey, out depth);
            RegistryKey rKey2 = _rKey.OpenSubKey(result,true);
            if (rKey2 != null)
            {
                if (textBox1.Text != String.Empty)
                {
                    RegistryKey newKey = rKey2.CreateSubKey(textBox1.Text);
                    newKey.Close();
                }
                else
                {
                    MessageBox.Show("Enter subkey name!");
                }
            }
            else
            {
                MessageBox.Show("Incorrect path!");
            }
            treeView1.Nodes.Clear();
            FillTree();
        }
        private void FillContextMenuStrip2()
        {
            ToolStripMenuItem changeParam = new ToolStripMenuItem("Change");
            ToolStripMenuItem deleteParam = new ToolStripMenuItem("Delete");
            contextMenuStrip2.Items.AddRange(new[] { changeParam, deleteParam, });
            dataGridView1.ContextMenuStrip = contextMenuStrip2;
            changeParam.Click += ChangeParam_Click;
            deleteParam.Click += DeleteParam_Click;
        }

        private void DeleteParam_Click(object? sender, EventArgs e)//check
        {
            RegistryKey _rKey;
            int depth;
            string result = GetPathNode(treeView1.SelectedNode, out _rKey, out depth);
            
            RegistryKey rKey2 = _rKey.OpenSubKey(result,true);
            if (rKey2 != null)
            {
                //Delete parameter
                string  valueStr = dataGridView1.CurrentCell.Value.ToString();
                var value = rKey2.GetValue(valueStr);

                if (value != null)
                {
                    rKey2.DeleteValue(valueStr);
                    rKey2.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect parameter name!");
                }

            }
            else
            {
                MessageBox.Show("Incorrect subkey path!");
            }
            treeView1.Nodes.Clear();
            FillTree();
        }

        private void ChangeParam_Click(object? sender, EventArgs e)//check
        {
            if (textBox2.Text != String.Empty && textBox3.Text != String.Empty)
            {
                RegistryKey _rKey;
                int depth;
                string result = GetPathNode(treeView1.SelectedNode, out _rKey, out depth);
                RegistryKey key = _rKey.OpenSubKey(result, true);
                if (key != null)
                {
                    string valueStr = dataGridView1.CurrentCell.Value.ToString();
                    var parameter = key.GetValue(valueStr);
                    if (parameter != null)
                    {
                        key.DeleteValue(valueStr);
                        key.SetValue(textBox2.Text, textBox3.Text);
                        key.Close();
                    }
                    else
                    {
                        MessageBox.Show("Incorrect path to parameter!");
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect path to subkey!");
                }
            }
            else
            {
                MessageBox.Show("Incorrect input!");
            }
            treeView1.Nodes.Clear();
            FillTree();
        }

        public static RegistryKey GetRegistryKey(string str) => str switch
        {
            "HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
            "HKEY_USERS" => Registry.Users,
            "HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
            "HKEY_CURRENT_USER" => Registry.CurrentUser,
            "HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
            _ => throw new ArgumentOutOfRangeException(nameof(str), $"Not expected direction value: {str}"),
        };
        private string GetPathNode(TreeNode treeNode, out RegistryKey _rKey, out int depth)
        {
            string _rKeyStr;
            if (treeNode.Level != 0)
            {
                _rKeyStr = treeNode.Parent.FullPath;
                _rKeyStr += "\\" + treeNode.Text;
            }
            else
            {
                _rKeyStr = treeNode.Text;
            }
            string[] str = _rKeyStr.Split("\\");
            depth = str.Length;
            _rKey = GetRegistryKey(str[0]);
            string result = "";
            if (str.Length > 1)
            {
                for (int i = 1; i < str.Length; i++)
                {
                    if (i == str.Length - 1)
                    { result += str[i]; }
                    else
                    { result += str[i] + '\\'; }
                }
            }
            return result;
        }
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            dataGridView1.Rows.Clear();
            RegistryKey _rKey;
            int depth;
            string result = GetPathNode(e.Node, out _rKey, out depth);
            if (depth>1)
            {
                RegistryKey _rKey2 = _rKey.OpenSubKey(result);
                if (_rKey2 != null)
                {
                    string[] vNames = _rKey2.GetValueNames();
                    for (int i = 0; i < vNames.Length; i++)
                    {
                        var value = _rKey2.GetValue(vNames[i]);

                        RegistryValueKind kind = _rKey2.GetValueKind(vNames[i]);
                        dataGridView1.Rows.Add(vNames[i], (RegistryValueKindNative)kind, value.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect path!");
                }
            }
            else
            {
                string[] vNames = _rKey.GetValueNames();
                for (int i = 0; i < vNames.Length; i++)
                {
                    var value = _rKey.GetValue(vNames[i]);

                    RegistryValueKind kind = _rKey.GetValueKind(vNames[i]);
                    dataGridView1.Rows.Add(vNames[i], (RegistryValueKindNative)kind, value.ToString());
                }
            }
        }
    }
}