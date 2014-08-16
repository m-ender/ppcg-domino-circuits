using System;
using System.Collections.Generic;

namespace dominoPrinter
{
	class Program
	{
		public static void Main(string[] args)
		{
//			string cStr = Console.ReadLine();
//			string erStr = Console.ReadLine();
//			int inputCount;
//			val[] vals = resolveVals(cStr, erStr, out inputCount);

			int inputCount;
			val[] vals = resolveVals(args[0], args[1], args[2], out inputCount);
			
//			foreach (val v in vals)
//			{
//				Console.WriteLine(v.strness);
//			}
			
			System.IO.StringWriter sw = new System.IO.StringWriter();
			orifnot(inputCount, vals, sw);
			System.IO.StringReader sr = new System.IO.StringReader(sw.ToString());
			
			printDominoes(sr, Console.Out, args.Length > 3 && args[3] == "quite");
		}
		
		public abstract class val
		{
			public int size;
			public bool[] rs;
		}
		
		public class baseVal : val
		{
			public bool b;
			public int id;
			
			public baseVal(int idN)
			{
				id = idN;
				size = 1;
			}
		}
		
		public abstract class biopVal : val
		{
			public val a, b;
			
			public biopVal(val aN, val bN)
			{
				a = aN;
				b = bN;
				size = a.size + b.size;
			}
			
			public bool buildCheckApply(nodev ntree)
			{
				nodev cur = ntree;
				rs = new bool[a.rs.Length];
				bool notOK = true;
				for (int i = 0; i < rs.Length; i++)
				{
					bool r = rs[i] = go(a.rs[i], b.rs[i]);
					if (notOK)
					{
						if (r)
						{
							if (cur.a == null)
								notOK = false;
							else
							{
								cur = cur.a;
								if (cur == nodev.full)
									return false;
							}
						}
						else
						{
							if (cur.b == null)
								notOK = false;
							else
							{
								cur = cur.b;
								if (cur == nodev.full)
									return false;
							}
						}
					}
				}
				
				ntree.apply(this, 0);
				return true;
			}
			
			public abstract bool go(bool a, bool b);
		}
		
		public class ifnotVal : biopVal
		{
			public override bool go(bool a, bool b)
			{
					return a ? false : b; // b IF NOT a, else FALSE
			}
			
			public ifnotVal(val aN, val bN) : base(aN, bN)
			{
			}
		}
		
		public class orval : biopVal
		{
			public override bool go(bool a, bool b)
			{
				return a || b; // a OR b
			}
			
			public orval(val aN, val bN) : base(aN, bN)
			{
			}
		}
		
		static bool boolCompare(bool[] a, bool b)
		{
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b)
				{
					return false;
				}
			}
			return true;
		}
		
		static bool boolFlat(bool[] a)
		{
			bool p = a[0];
			for (int i = 1; i < a.Length; i++)
			{
				if (a[i] != p)
					return false;
			}
			return true;
		}
		
		static bool boolCompare(bool[] a, bool[] b)
		{
			if (a.Length != b.Length)
				return false; // let's do this proeprly
			for (int i = 0; i < a.Length; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}
		
		// solver
		
		// these is something VERY WRONG with the naming in this code
		public class nodev
		{
			public static nodev full = new nodev();
			
			public nodev a, b;
			
			public nodev()
			{
				a = null;
				b = null;
			}
			
			public bool contains(bool[] rs)
			{
				nodev cur = this;
				if (cur == full)
					return true;
				
				for (int i = 0; i < rs.Length; i++)
				{
					if (rs[i])
					{
						if (cur.a == null)
							return false;
						cur = cur.a;
					}
					else
					{
						if (cur.b == null)
							return false;
						cur = cur.b;
					}
					
					if (cur == full)
						return true;
				}
				return true;
			}
			
			public bool contains(val v)
			{
				nodev cur = this;
				if (cur == full)
					return true;
				
				for (int i = 0; i < v.rs.Length; i++)
				{
					if (v.rs[i])
					{
						if (cur.a == null)
							return false;
						cur = cur.a;
					}
					else
					{
						if (cur.b == null)
							return false;
						cur = cur.b;
					}
					
					if (cur == full)
						return true;
				}
				return true;
			}
			
			// returns whether it's full or not
			public bool apply(val v, int idx)
			{
				if (v.rs[idx])
				{
					if (a == null)
					{
						if (idx == v.rs.Length - 1)
						{ // end of the line, fellas
							a = full;
							if (b == full)
								return true;
							return false;
						}
						else
						{
							a = new nodev();
						}
					}
					if (a.apply(v, idx + 1))
						a = full;
					if (a == full && b == full)
						return true;
				}
				else
				{
					if (b == null)
					{
						if (idx == v.rs.Length - 1)
						{ // end of the line, fellas
							b = full;
							if (a == full)
								return true;
							return false;
						}
						else
						{
							b = new nodev();
						}
					}
					if (b.apply(v, idx + 1))
						b = full;
					if (a == full && b == full)
						return true;
				}
				return false;
			}
		}
		
		public static void sortOutIVals(baseVal[] ivals, int rc)
		{
			for (int i = 0; i < ivals.Length; i++)
			{
				ivals[i].rs = new bool[rc];
				ivals[i].b = false;
			}
			
			int eri = 0;
			
			goto next;
		again:
			for (int i = ivals.Length - 1; i >= 0; i--)
			{
				if (ivals[i].b == false)
				{
					ivals[i].b = true;
					goto next;
				}
				ivals[i].b = false;
			}
			
			return;
		next:
			for (int i = ivals.Length - 1; i >= 0; i--)
			{
				ivals[i].rs[eri] = ivals[i].b;
			}
			
			eri++;
			goto again;
		}
		
		public static val resolveBoooooooring(int inputCount, int c, int o)
		{
			val tval = new baseVal(-1);
			val fval = new baseVal(-2);
			tval.rs = new bool[c];
			fval.rs = new bool[c];
			for (int i = 0; i < c; i++)
			{
				tval.rs[i] = true;
				fval.rs[i] = false;
			}
			
			val res = tval;
			
			baseVal[] ivals = new baseVal[inputCount];
			
			for (int i = 0; i < inputCount; i++)
			{
				ivals[i] = new baseVal(i); // value will change anyway
			}
			
			sortOutIVals(ivals, c);
			
			for (int i = 0; i < ivals.Length; i++)
			{
				val cur = ivals[i];
				if (cur.rs[o])
				{
					cur = new ifnotVal(cur, tval);
				}
				
				res = new ifnotVal(cur, res); // don't forget to try putting the not in before this for fun/evidence sake
			}
			
			return res;
		}
		
		public static val[] resolve(int inputCount, int c, bool[][] erss)
		{
			val[] res = new val[erss.Length];
			
			List<List<val>> bvals = new List<List<val>>();
			nodev ntree = new nodev();
			
			List<val> nvals = new List<val>();
			
			val tval = new baseVal(-1);
			val fval = new baseVal(-2);
			baseVal[] ivals = new baseVal[inputCount];
			
			for (int i = 0; i < inputCount; i++)
			{
				ivals[i] = new baseVal(i); // value will change anyway
			}
			
			sortOutIVals(ivals, c);
			
			for (int i = 0; i < inputCount; i++)
			{
				nvals.Add(ivals[i]);
			}
			
			tval.rs = new bool[c];
			fval.rs = new bool[c];
			for (int i = 0; i < c; i++)
			{
				tval.rs[i] = true;
				fval.rs[i] = false;
			}
			
			nvals.Add(tval);
			nvals.Add(fval); // ifnot and or do nothing with falses
			
			bvals.Add(new List<val>());
			
			foreach (val v in nvals)
			{
				ntree.apply(v, 0);
				if (!boolFlat(v.rs))
					bvals[0].Add(v); // I trust these are distinct..
			}
			
			Func<biopVal, bool> checkValb = (v) =>
			{
				if (!v.buildCheckApply(ntree))
				{
					return false;
				}
				bvals[v.size-1].Add(v);
				return true;
			};
			
			Action<biopVal, List<val>> checkVal = (v, li) =>
			{
				if (checkValb(v))
					li.Add(v);
			};
			
			int maxSize = 1;
			
		again:
			for (int i = 0; i < erss.Length; i++)
			{
				bool[] ers = erss[i];
				if (res[i] == null && ntree.contains(ers))
				{
					// there is a reason this is separate... I'm sure there is....
					foreach (val rv in nvals)
					{
						if (boolCompare(rv.rs, ers))
						{
							res[i] = rv;
							break;
						}
					}
				}
			}
			
			for (int i = 0; i < erss.Length; i++)
			{
				if (res[i] == null)
					goto notoveryet;
			}
			return res;
			
		notoveryet:
			
			maxSize++;
			bvals.Add(new List<val>()); // bvals[maxSize-1] always exists
			
			nvals.Clear();
			long cc = 0;
			
			List<val> sbvals = bvals[maxSize - 2];
			// NOTs have a habit of working out, get it checked first
			for (int i = sbvals.Count - 1; i >= 0; i--)
			{ // also known as nvals, but let's ignore that
				val arv = sbvals[i];
				checkVal(new ifnotVal(arv, tval), nvals);
				cc += 1;
			}
			
			for (int s = 1; s < maxSize; s++)
			{
				List<val> abvals = bvals[s - 1];
				int t = maxSize - s;
				if (t < s)
					break;
				List<val> bbvals = bvals[t - 1];
				
				for (int i = abvals.Count - 1; i >= 0; i--)
				{
					val arv = abvals[i];
					
					int jt = t == s ? i : bbvals.Count - 1;
					for (int j = jt; j >= 0; j--)
					{
						val brv = bbvals[j];
						
						checkVal(new ifnotVal(brv, arv), nvals);
						checkVal(new ifnotVal(arv, brv), nvals);
						checkVal(new orval(brv, arv), nvals); // don't technically need ors, but they are good fun
						cc += 3;
					}
				}
			}
			
			int bc = 0;
			foreach (List<val> bv in bvals)
				bc += bv.Count;
			//Console.Error.WriteLine(nvals.Count + " - " + bc + " - " + cc);
			goto again;
		}
		
		public static val[] resolveVals(string cStr, string erStr, out int inputCount)
		{
			string[] data = cStr.Split(' ');
			int ic = int.Parse(data[0]);
			int oc = int.Parse(data[1]);
			inputCount = ic;
			if (inputCount < 5)
				return resolveVals(ic, oc, erStr);
			else
				return resolveValsBoooring(ic, oc, erStr);
		}
		
		public static val[] resolveVals(string mStr, string nStr, string erStr, out int inputCount)
		{
			int ic = int.Parse(mStr);
			int oc = int.Parse(nStr);
			inputCount = ic;
			if (inputCount <= 4)
				return resolveVals(ic, oc, erStr);
			else
				return resolveValsBoooring(ic, oc, erStr);
		}
		
		public static val resolveByOrViolence(val[] bvals, int c, bool[] ers)
		{
			val cur = new baseVal(-2);
			bool iszero = true;
			cur.rs = new bool[c]; // let be falses
			
			for (int i = c - 1; i >= 0; i--)
			{
				if (ers[i])
				{
					if (iszero)
					{
						iszero = false;
						cur = bvals[i];
					}
					else
						cur = new orval(cur, bvals[i]);
				}
			}
			
			return cur;
		}
		
		public static val[] resolveValsBoooring(int inputCount, int outputCount, string erStr)
		{	
			// boring
			
			int c = 1;
			for (int i = 0; i < inputCount; i++)
			{
				c *= 2;
			}
			
			val[] bvals = new val[c];
			for (int i = 0; i < c; i++)
			{
				bvals[i] = resolveBoooooooring(inputCount, c, i);
			}
			
			// boring
			val[] res = new val[outputCount];
			
			string[] data = erStr.Split(',');
			for (int i = 0; i < outputCount; i++)
			{
				bool[] ers = new bool[data.Length];
				for (int j = 0; j < data.Length; j++)
					ers[j] = data[j][i] == '1';
				res[i] = resolveByOrViolence(bvals, c, ers);
			}
			
			return res;
		}
		
		public static val[] resolveVals(int inputCount, int outputCount, string erStr)
		{
			val[] res;
			
			string[] data = erStr.Split(',');
			bool[][] erss = new bool[outputCount][];
			for (int i = 0; i < outputCount; i++)
			{
				bool[] ers = new bool[data.Length];
				for (int j = 0; j < data.Length; j++)
					ers[j] = data[j][i] == '1';
				erss[i] = ers;
			}
			
			res = resolve(inputCount, data.Length, erss);
			
			return res;
		}
		
		// organiser
		public class vnode
		{
			private static vnode[] emptyVC = new vnode[0];
			public static vnode oneVN = new vnode('1');
			public static vnode noVN = new vnode(' ');
			public static vnode flatVN = new vnode('_');
			public static vnode moveUpVN = new vnode('/');
			public static vnode moveDownVN = new vnode('\\');
			public static vnode inputVN = new vnode('I');
			public static vnode outputVN = new vnode('O');
			public static vnode swapVN = new vnode('X');
			public static vnode splitDownVN = new vnode('v');
			
			public int size;
			public vnode[] children;
			public char c;
			public int id = -3;
			
			public vnode(char cN)
			{
				c = cN;
				children = emptyVC;
				size = 1;
			}
			
			public vnode(val v)
			{
				biopVal bv = v as biopVal;
				
				if (bv != null)
				{
					children = new vnode[2];
					children[0] = new vnode(bv.a);
					children[1] = new vnode(bv.b);
					size = children[0].size + children[1].size;
					
					if (bv is orval)
						c = 'U';
					if (bv is ifnotVal)
						c = 'u';
				}
				else
				{
					children = emptyVC;
					size = 1;
					c = 'I';
					id = ((baseVal)v).id;
				}
			}
		}
		
		public class nonArray<T>
		{
			public int w = 0, h = 0;
			Dictionary<int, Dictionary<int, T>> map;
			
			public nonArray()
			{
				map = new Dictionary<int, Dictionary<int, T>>();
			}
			
			public T this[int x, int y]
			{
				get
				{
					Dictionary<int, T> yd;
					if (map.TryGetValue(x, out yd))
					{
						T v;
						if (yd.TryGetValue(y, out v))
						{
							return v;
						}
					}
					return default(T);
				}
				set
				{
					if (x >= w)
						w = x + 1;
					if (y >= h)
						h = y + 1;
					Dictionary<int, T> yd;
					if (map.TryGetValue(x, out yd))
					{
						yd[y] = value;
					}
					else
					{
						map[x] = new Dictionary<int, T>();
						map[x][y] = value;
					}
				}
			}
		}
		
		public static int fillOutMap(nonArray<vnode> map, vnode rn, int y, int x)
		{
			if (rn.children.Length == 0)
			{
				map[y,x] = rn;
				return 1;
			}
			else
			{
				map[y+1,x] = rn;
				for (int i = 0; i < rn.children.Length; i++)
				{
					
					if (i == 0)
					{
						fillOutMap(map, rn.children[i], y, x + 1);
					}
					
					if (i == 1)
					{
						int ex = x + rn.children[0].size;
						for (int j = 1; j < ex - x; j++)
							map[y - j + 1,ex - j] = vnode.moveUpVN;
						fillOutMap(map, rn.children[i], y, ex);
					}
					
					y += rn.children[i].size;
				}
			}
			
			return rn.size;
		}
		
		public static void orifnot(int inputCount, val[] vals, System.IO.TextWriter writer)
		{
			// step one - build weird tree like thing of death
			nonArray<vnode> map = new nonArray<vnode>();
			
			int curY = 0;
			foreach (val v in vals)
			{
				vnode vnt = new vnode(v);
				map[curY, 0] = vnode.outputVN;
				curY += fillOutMap(map, vnt, curY, 1);
			}
			
			// step two - build the thing to get the values to where they need to be
			// find Is
			List<int> tis = new List<int>();
			for (int y = 0; y < map.w; y++)
			{
				for (int x = map.h - 1; x >= 0; x--)
				{
					vnode vn = map[y,x];
					if (vn != null && vn.c == 'I')
					{
						tis.Add(vn.id);
						if (vn.id > -2)
						{
							for (;x < map.h; x++)
							{
								map[y,x] = vnode.flatVN;
							}
						}
						goto next;
					}
				}
				tis.Add(-2);
			next:
				continue;
			}
			
			// I do not like this piece of code, it can be replaced further down for the better if you get round to thinking about it
			// add unused Is
			for (int z = 0; z < inputCount; z++)
			{
				if (!tis.Contains(z))
				{
					int midx = tis.IndexOf(-2);
					if (midx != -1)
					{
						tis[midx] = z;
						//System.Console.Error.WriteLine("swap in " + z);
					}
					else
					{
						tis.Add(z);
						//System.Console.Error.WriteLine("add on " + z);
						map[map.h-1,map.w] = vnode.flatVN;
					}
				}
			}
			
			int curX = map.h;
			
		MORE:
			for (int y = 0; y < map.w; y++)
			{
				if (y == map.w - 1)
				{
					if (tis[y] == -2)
						map[y,curX] = vnode.noVN;
					else
						map[y,curX] = vnode.flatVN;
				}
				else
				{
					int prev = tis[y];
					int cur = tis[y + 1];
					
					if (cur != -2 && (prev == -2 || cur < prev))
					{ // swap 'em
						map[y,curX] = vnode.noVN;
						if (prev == -2)
							map[y+1,curX] = vnode.moveDownVN;
						else
							map[y+1,curX] = vnode.swapVN;
						int temp = tis[y];
						tis[y] = tis[y + 1];
						tis[y + 1] = temp;
						y++; // skip
					}
					else
					{
						if (/*thatThingThat'sAThing*/ prev == cur && cur != -2)
						{
							map[y,curX] = vnode.noVN;
							map[y+1,curX] = vnode.splitDownVN;
							int temp = tis[y];
							tis[y+1] = -2;
							y++; // skip
						}
						else
						{
							if (prev == -2)
								map[y,curX] = vnode.noVN;
							else
								map[y,curX] = vnode.flatVN;
						}
					}
				}
			}
			
			// check if sorted
			for (int y = 0; y < map.w - 1; y++)
			{
				int prev = tis[y];
				int cur = tis[y + 1];
				
				if (cur != -2 && (prev == -2 || cur < prev))
					goto NOTSORTED;
			}
			
			goto WHATNOW;
			
		NOTSORTED:
			curX++;
			goto MORE;
			
		WHATNOW:
			
			tis.Add(-2); // this is to avoid boud checking y+2
			// so... it's sorted now, so add the splits
		morePlease:
			curX++;
			for (int y = 0; y < map.w; y++)
			{
				if (y == map.w - 1)
				{
					if (tis[y] == -2)
						map[y,curX] = vnode.noVN;
					else
						map[y,curX] = vnode.flatVN;
				}
				else
				{
					int prev = tis[y];
					int cur = tis[y + 1];
					int next = tis[y + 2];
					
					if (cur != -2 && prev == cur && cur != next)
					{ // split
						map[y,curX] = vnode.noVN;
						map[y+1,curX] = vnode.splitDownVN;
						tis[y + 1] = -2;
						y++; // skip
					}
					else
					{
						if (prev == -2)
							map[y,curX] = vnode.noVN;
						else
							map[y,curX] = vnode.flatVN;
					}
				}
			}
			
			// check if collapsed
			for (int y = 0; y < map.w - 1; y++)
			{
				int prev = tis[y];
				int cur = tis[y + 1];
				
				if (cur != -2 && prev == cur)
					goto morePlease;
			}
			
			// ok... now we put in the Is and 1
			curX++;
			map[0, curX] = vnode.oneVN;
			int eyeCount = 0;
			int ly = 0;
			for (int y = 0; y < map.w; y++)
			{
				if (tis[y] > -1)
				{
					map[y, curX] = vnode.inputVN;
					eyeCount++;
					ly = y;
				}
			}
			
			/* moved UP
			// add extra unused Is per spec
			for (;eyeCount < inputCount; eyeCount++)
			{
				ly++;
				map[ly, curX] = vnode.inputVN;
			}
			*/
			
			// step three - clean up if we can
			// push back _  esq things to  _
			//           _/               /
			// this /shouldn't/ be necessary if I compact the vals properlu
			for (int y = 0; y < map.w - 1; y++)
			{
				for (int x = 1; x < map.h; x++)
				{
					if (map[y, x] != null && map[y+1, x] != null && map[y+1, x-1] != null)
					{
						char uc = map[y+1, x-1].c;
						if (map[y, x].c == '_' && map[y+1, x].c == '_'
						    && (uc == 'U' || uc == 'u'))
						{
							map[y, x] = vnode.noVN;
							map[y, x-1] = vnode.flatVN;
							map[y+1, x] = map[y+1, x-1];
							map[y+1, x-1] = vnode.noVN;
						}
					}
				}
			}
			
			// step four - write out map
			writer.WriteLine(map.h + " " + map.w);
			
			for (int y = 0; y < map.w; y++)
			{
				for (int x = map.h - 1; x >= 0; x--)
				{
					vnode vn = map[y,x];
					if (vn != null)
						writer.Write(vn.c);
					else
						writer.Write(' ');
				}
				writer.WriteLine();
			}
		}
		
		// printer
		static string up1 =				@"      /     /     /     /";
		static string input =			@"                    |||||";
		static string output =			@"                    |    ";
		static string flat =			@"            |/  \  /|\   ";
		static string splitDown = 		@"|//   / /\  |\/    /     ";
		static string splitUp = 		@"         \  |/\ \ \/|\\  ";
		static string moveDown = 		@"|//     /     /    /     ";
		static string moveUp = 			@"         \    \   \ |\\  ";
		static string swap =	 		@"|/  |  /\   /\   \/ |\  |";
		static string orDown =	 		@"|/    /     |/  \  /|\   ";
		static string orUp =	 		@"|/    /  \  |\  \   |\   ";
		static string ifnotDown =	 	@"|/     /     -   \/ |\  |";
		static string ifnotUp =	 		@"|/  |  /\    -   \  |\   ";
		
		public static void printDominoes(System.IO.TextReader reader, System.IO.TextWriter writer, bool moreverbosemaybe)
		{
			string line;
			string[] data;
			
			line = reader.ReadLine();
			data = line.Split(' ');
			int w = int.Parse(data[0]);
			int h = int.Parse(data[1]);
			
			int ox = 0;
			int oy = 0;
			int cx = 5;
			int cy = 5;
			
			char[,] T = new char[ox + w * cx, oy + h * (cy - 1) + 1];
			
			Action<int, int, string> setBlock = (int x, int y, string str) =>
			{
				for (int i = 0; i < cx; i++)
				{
					for (int j = 0; j < cy; j++)
					{
						char c = str[i + j * cx];
						if (c != ' ')
							T[ox + x * cx + i, oy + y * (cy - 1) + j] = c;
					}
				}
			};
			
			// read and write
			for (int j = 0; j < h; j++)
			{
				line = reader.ReadLine();
				for (int i = 0; i < w; i++)
				{
					if (line[i] != ' ')
					{
						switch (line[i])
						{
							case '1':
								setBlock(i, j, up1);
								break;
							case '_':
								setBlock(i, j, flat);
								break;
							case '^':
								setBlock(i, j, splitUp);
								break;
							case 'v':
								setBlock(i, j, splitDown);
								break;
							case '/':
								setBlock(i, j, moveUp);
								break;
							case '\\':
								setBlock(i, j, moveDown);
								break;
							case 'X':
								setBlock(i, j, swap);
								break;
							case 'U':
								setBlock(i, j, orUp);
								break;
							case 'D':
								setBlock(i, j, orDown);
								break;
							case 'u':
								setBlock(i, j, ifnotUp);
								break;
							case 'd':
								setBlock(i, j, ifnotDown);
								break;
							case 'I':
								setBlock(i, j, input);
								break;
							case 'O':
								setBlock(i, j, output);
								break;
						}
					}
				}
			}
			
			// end
			for (int i = 0; i < T.GetLength(0); i++)
			{
				T[i, 0] = '/';
			}
			
			// writeout
			w = T.GetLength(0) - cx + 1;
			h = T.GetLength(1);
			if (moreverbosemaybe)
				writer.Write(w + " " + h + " ");
			for (int j = 0; j < T.GetLength(1); j++)
			{
				for (int i = 0; i < T.GetLength(0) - cx + 1; i++)
				{
					char c = T[i, j];
					writer.Write(c == 0 ? ' ' : c);
				}
				if (!moreverbosemaybe)
					writer.WriteLine();
			}
		}
	}
}