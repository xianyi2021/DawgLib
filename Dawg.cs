using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawgLib
{
    public class Dawg
    {
        public Dawg()
        {
            this.End.CanStop = true;
        }
        public List<DawgNode> Register = new List<DawgNode>();
        public DawgNode Start = new DawgNode(null);
        public DawgNode End = new DawgNode(null);

        public bool Search(string word)
        {
            return Search(Start, word, 0);
        }

        private bool Search(DawgNode node, string word, int p)
        {
            char c=word[p];
            DawgNode next = null;
            if (node.FollowingNodes.TryGetValue(c, out next))
            {
                if (p == word.Length - 1)
                    return next.CanStop;
                else
                    return Search(next, word, p + 1);
            }
            else
                return false;
        }
        public class DawgNode
        {
            static int SNSeed = 0;
            public int SN;
            public DawgNode(string backkey)
            {
                SNSeed++;
                this.SN = SNSeed;
                this.BackKey = backkey;
            }
            public bool CanStop;
            public string BackKey;
            public SortedList<char, DawgNode> FollowingNodes = new SortedList<char, DawgNode>();

            public override string ToString()
            {
                string text = "DawgNode " + this.SN;
                if (this.CanStop)
                    text += "(Stop)";
                text += ":";
                IEnumerator<KeyValuePair<char, DawgNode>> enumerator = this.FollowingNodes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    text += enumerator.Current.Key + "->" + enumerator.Current.Value.SN + ";";
                }
                return text;// +" '" + this.BackKey + "'";
            }
        }
        public class IndexTreeEdge
        {
            //for debugging
            static int SNSeed = 0;
            public int SN;

            public DawgNode UpDawgNode;
            public DawgNode DownDawgNode;
            public char Character;
            public IndexTreeEdge(IndexTreeEdge parent, char c)
            {
                SNSeed++;
                this.SN = SNSeed;
                this.Character = c;
                this.Parent = parent;
            }
            public IndexTreeEdge Parent;
            public SortedList<char, IndexTreeEdge> Children = new SortedList<char, IndexTreeEdge>();
            public bool CanStop = false;
            public override string ToString()
            {
                IndexTreeEdge node = this;
                string text = "";
                if (this.CanStop)
                    text += "(Stop)";
                while (node != null && node.Character != '\0')
                {
                    text = node.Character + text;
                    node = node.Parent;
                }

                return "IndexTreeNode " + this.SN + " '" + this.Character + "' of '" + text + "'";
            }
        }
        public class Constructor
        {
            internal Dawg Construct(string[] words)
            {
                IndexTreeEdge roottreeedge = IndexWords(words);
                return Deduce(roottreeedge);
            }
            public IndexTreeEdge IndexWords(string[] words)
            {
                IndexTreeEdge root = new IndexTreeEdge(null, '\0');
                foreach (string word in words)
                {
                    IndexWord(root, word, 0);
                }
                return root;
            }

            private IndexTreeEdge IndexWord(IndexTreeEdge parent, string word, int depth)
            {
                String s = parent.ToString();
                char c = word[depth];
                IndexTreeEdge child = null;
                if (!parent.Children.TryGetValue(c, out child))
                {
                    child = new IndexTreeEdge(parent, c);
                    parent.Children.Add(c, child);
                }
                if (depth == word.Length - 1)
                {
                    child.CanStop = true;
                    return child;
                }
                else
                {
                    IndexTreeEdge grand = IndexWord(child, word, depth + 1);
                }
                return child;
            }


            Dawg Deduce(IndexTreeEdge root)
            {
                Dawg dawg = new Dawg();
                root.UpDawgNode = dawg.Start;
                Deduce(dawg, root);
                return dawg;
            }

            private void Deduce(Dawg dawg, IndexTreeEdge treeedge)
            {
                if (treeedge.Children.Count == 0)//a leaf
                {
                    DeduceBackward(dawg, dawg.End, treeedge);
                }
                foreach (IndexTreeEdge child in treeedge.Children.Values)
                {
                    Deduce(dawg, child);
                }
            }

            private DawgNode GetEqualNode(Dawg dawg, IndexTreeEdge treeedge)
            {
                foreach (DawgNode exitnode in dawg.Register)
                {
                    if (exitnode.FollowingNodes.Count != treeedge.Parent.Children.Count)
                        continue;
                    if (exitnode.CanStop != treeedge.Parent.CanStop)
                        continue;
                    List<KeyValuePair<char, DawgNode>> pairs = exitnode.FollowingNodes.ToList();
                    List<KeyValuePair<char, IndexTreeEdge>> treepairs = treeedge.Parent.Children.ToList();
                    int i = 0;
                    for (; i < exitnode.FollowingNodes.Count; i++)
                    {
                        if (pairs[i].Key != treepairs[i].Key)
                            break;
                        if (pairs[i].Value != treepairs[i].Value.DownDawgNode)
                            break;
                    }
                    if (i < exitnode.FollowingNodes.Count)
                        continue;
                    return exitnode;
                }
                return null;
            }
            private void DeduceBackward(Dawg dawg, DawgNode prev, IndexTreeEdge treeedge)
            {
                treeedge.DownDawgNode = prev;
                if (treeedge.CanStop)
                    prev.CanStop = true;
                if (treeedge.Parent.Character == '\0')
                {//首TreeNode
                    if (!dawg.Start.FollowingNodes.ContainsKey(treeedge.Character))
                        dawg.Start.FollowingNodes.Add(treeedge.Character, prev);
                    return;
                }
                DawgNode current = null;
                if (treeedge.Parent.DownDawgNode != null)
                {
                    current = treeedge.Parent.DownDawgNode;
                    current.FollowingNodes.Add(treeedge.Character, prev);
                }
                else if (treeedge.Children.Count > 1)
                {//分支TreeNode，直接创建DawgNode
                    current = new DawgNode(null);
                    dawg.Register.Add(current);
                    current.FollowingNodes.Add(treeedge.Character, prev);
                }
                else
                {
                    current = GetEqualNode(dawg, treeedge);
                    if (current == null)
                    {
                        current = new DawgNode(null);
                        dawg.Register.Add(current);
                        current.FollowingNodes.Add(treeedge.Character, prev);
                    }
                }
                treeedge.UpDawgNode = current;
                DeduceBackward(dawg, current, treeedge.Parent);
            }

        }
    }
}
