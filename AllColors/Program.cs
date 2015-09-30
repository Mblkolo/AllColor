using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllColors
{
    struct Rgb
    {
        public byte R;
        public byte G;
        public byte B;
        public readonly bool isAssigned;

        public Rgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            isAssigned = true;
        }

        public int QDist(Rgb color)
        {
            return (color.R - R) * (color.R - R) + (color.G - G) * (color.G - G) + (color.B - B) * (color.B - B);
        }

        public override string ToString()
        {
            return String.Format("{{{0}, {1}, {2}}}", R, G, B);
        }
    }



    class VpNode
    {
        public int Radius;
        public VpNode Insade;
        public VpNode Outsade;
        public Rgb Value;

        public bool IsLeaf
        {
            get { return Outsade == null || Insade == null; }
        }
    }

    static class VpTree
    {
        public static VpNode CreateTree(Rgb[] colors)
        {
            var root = new VpNode();
            
            var candidats = new Stack<Tuple<VpNode, Rgb[]>>();
            candidats.Push(Tuple.Create(root, colors));
            while (candidats.Count > 0)
            {
                var candidat = candidats.Pop();
                var node = candidat.Item1;
                var allColors = candidat.Item2;
                var color = allColors[0];

                if (allColors.Length == 1)
                {
                    node.Value = color;
                    continue;
                }

                if(allColors.Length == 2)
                {
                    node.Value = color;
                    node.Radius = allColors[1].QDist(color);
                    node.Insade = new VpNode();

                    candidats.Push(Tuple.Create(node.Insade, new Rgb[]{ allColors [1]}));
                    continue;
                }

                var sorted = allColors.Skip(1).OrderBy(x => color.QDist(x)).ToArray();
                var medianIndex = (sorted.Length - 1) / 2; //2 - 0, 3 - 1, 4 - 1, 5 - 2

                var inside = new VpNode();
                var insideColors = new Rgb[medianIndex + 1];
                Array.Copy(sorted, insideColors, medianIndex + 1);
                candidats.Push(Tuple.Create(inside, insideColors));

                var outside = new VpNode();
                var outsideColors = new Rgb[sorted.Length - (medianIndex + 1)];
                Array.Copy(sorted, medianIndex + 1, outsideColors, 0, sorted.Length - (medianIndex + 1));
                candidats.Push(Tuple.Create(outside, outsideColors));

                node.Value = color;
                node.Radius = sorted[medianIndex].QDist(color);
                node.Insade = inside;
                node.Outsade = outside;
            }

            return root;
        }

        public static VpNode Near(VpNode root, Rgb target)
        {
            int bestQDist = int.MaxValue;
            VpNode bestCandidat = root;

            var candidats = new Stack<VpNode>();
            candidats.Push(root);
            while(candidats.Count>0)
            {
                var node = candidats.Pop();
                var dist = node.Value.QDist(target);
                if(dist < bestQDist)
                {
                    bestQDist = dist;
                    bestCandidat = node;
                }

                if (node.Insade != null && !(dist > bestQDist + node.Radius))
                    candidats.Push(node.Insade);

                if (node.Outsade != null && !(dist < node.Radius - bestQDist))
                    candidats.Push(node.Outsade);
            }

            return bestCandidat;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            //2-8
            const int colorBits = 2;
            const int colorCount = 1 << (colorBits - 1);

            var allColors = new List<Rgb>();
            for (int r = 0; r < colorCount; ++r)
                for (int g = 0; g < colorCount; ++g)
                    for (int b = 0; b < colorCount; ++b)
                        allColors.Add(new Rgb((byte)(r * 256 / colorCount), (byte)(g * 256 / colorCount), (byte)(b * 256 / colorCount)));

            var tree = VpTree.CreateTree(allColors.ToArray());

            Console.WriteLine(VpTree.Near(tree, new Rgb(0, 0, 0)).Value);
            Console.WriteLine(VpTree.Near(tree, new Rgb(1, 1, 1)).Value);
            Console.WriteLine(VpTree.Near(tree, new Rgb(255, 200, 200)).Value);
        }
    }
}
