using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

    struct YX
    {
        public readonly ushort X;
        public readonly ushort Y;
        private readonly int hash;

        public YX(ushort y, ushort x)
        {
            X = x;
            Y = y;
            hash = (y << 16) | x;
        }

        public override int GetHashCode()
        {
            return hash;
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //        return false;

        //    if ( !(obj is XY))
        //        return false;

        //    var p = (XY)obj;
        //    return (X == p.X) && (Y == p.Y);
        //}


    }

    class VpNode
    {
        public int Radius;
        public VpNode Insade;
        public VpNode Outsade;
        public Rgb Value;

        public bool IsUsed;
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

                if (allColors.Length == 2)
                {
                    node.Value = color;
                    node.Radius = allColors[1].QDist(color);
                    node.Insade = new VpNode();

                    candidats.Push(Tuple.Create(node.Insade, new Rgb[] { allColors[1] }));
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
            VpNode bestCandidat = null;

            var candidats = new Stack<VpNode>();
            candidats.Push(root);
            while (candidats.Count > 0)
            {
                var node = candidats.Pop();

                var dist = node.Value.QDist(target);
                if (dist < bestQDist && !node.IsUsed)
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
            const int width = 2;
            const int height = colorCount * colorCount * colorCount / 2;

            var allColors = new List<Rgb>();
            for (int r = 0; r < colorCount; ++r)
                for (int g = 0; g < colorCount; ++g)
                    for (int b = 0; b < colorCount; ++b)
                        allColors.Add(new Rgb((byte)(r * 256 / colorCount), (byte)(g * 256 / colorCount), (byte)(b * 256 / colorCount)));

            var tree = VpTree.CreateTree(allColors.ToArray());


            var front = new Stack<YX>();

            var neighbors = new YX[8];
            var canvas = new Rgb[height, width];
            foreach (var color in allColors)
            {
                if(front.Count == 0)
                {
                    canvas[0, 0] = color;
                    front.Push(new YX(1, 0));
                    front.Push(new YX(0, 1));
                    continue;
                }

                var f = front.Pop();
                while (canvas[f.Y, f.X].isAssigned)
                    f = front.Pop();

                int r = 0, g = 0, b = 0, count = 0;

                neighbors[0] = new YX((ushort)(f.Y - 1), (ushort)(f.X - 1));
                neighbors[1] = new YX((ushort)(f.Y - 1), (ushort)(f.X + 0));
                neighbors[2] = new YX((ushort)(f.Y - 1), (ushort)(f.X + 1));
                neighbors[3] = new YX((ushort)(f.Y + 0), (ushort)(f.X - 1));
                neighbors[4] = new YX((ushort)(f.Y + 0), (ushort)(f.X + 1));
                neighbors[5] = new YX((ushort)(f.Y + 1), (ushort)(f.X - 1));
                neighbors[6] = new YX((ushort)(f.Y + 1), (ushort)(f.X + 0));
                neighbors[7] = new YX((ushort)(f.Y + 1), (ushort)(f.X + 1));
                foreach (var n in neighbors)
                {
                    if (n.X < 0 || n.Y < 0 || n.X >= width || n.Y >= height)
                        continue;

                    var c = canvas[n.Y, n.X];
                    if (!c.isAssigned)
                    {
                        front.Push(n);
                        continue;
                    }

                    r += c.R;
                    g += c.G;
                    b += c.B;
                    count++;
                }
                if (count > 0)
                {
                    r /= count;
                    g /= count;
                    b /= count;
                }

                VpNode node = VpTree.Near(tree, new Rgb((byte)r, (byte)g, (byte)b));
                node.IsUsed = true;
                canvas[f.Y, f.X] = node.Value;


                using (var bitmap = new Bitmap(width, height))
                {
                    for(int y=0; y<height; ++y)
                        for(int x =0; x<width; ++x)
                        {
                            var c = canvas[y, x];
                            bitmap.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B));
                        }

                    Directory.CreateDirectory("Out");
                    bitmap.Save("Out\\Image_" + DateTime.Now.Ticks + ".png");
                }
            }
        }
    }
}
