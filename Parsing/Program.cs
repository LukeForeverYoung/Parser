using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
	class Program
	{
		static Token[] ReadToken(String filePath)
		{
			List<Token> list = new List<Token>();
			String str;
			try
			{
				FileStream fs = new FileStream(filePath, FileMode.Open);
				StreamReader rs = new StreamReader(fs);
				int line = 0;
				while(!rs.EndOfStream)
				{
					str = rs.ReadLine();
					if (str.Split(' ')[0] != @"\n")
						list.Add(new Token(str.Split(' ')[0], Int32.Parse(str.Split(' ')[1]), line));
					else
						line++;
				}
				rs.Close();
				fs.Close();
			}
			catch (Exception e) { throw e; }  
			return list.ToArray();
		}
		static void Main(string[] args)
		{
			ParsingMachine machine = new ParsingMachine(ReadToken(@"src/tokens.txt"));
			if (machine.Run())
				Console.WriteLine("Accepted!");
			else
				Console.WriteLine("Wrong!");
		}
			
	}
	class ParsingMachine
	{
		Token[] tokens;
		Dictionary<int, SymbolInterface> map;
		interface SymbolInterface { }
		class TerminalSymbol : SymbolInterface
		{
			public int code { get; set; }
			public TerminalSymbol(int code)
			{
				this.code = code;
			}
		}
		abstract class Non_TerminalSymbol : SymbolInterface
		{
			protected Dictionary<int,int[]> edge;
			public static SymbolInterface CreateNon_TerminalSymbol(int value)
			{
				switch(value)
				{
					case -2:
						return new S();
					case -3:
						return new E();
					case -4:
						return new Ex();
					case -5:
						return new F();
				}
				return null;
			}
			protected Non_TerminalSymbol()
			{
				edge = new Dictionary<int, int[]>();
			}
			public int[] First(int key)
			{
				if (edge.ContainsKey(key))
					return edge[key];
				else return null;
			}
		}
		/**
		 * Grammar List
		 * S	:	-2
		 * E	:	-3
		 * E'	:	-4
		 * F	:	-5
		 * EOF	:	-1
		 * NA	:	0  void
		 * B	:	11 word
		 * N	:	12 number
		 * +	:	13
		 * -	:	14
		 * *	:	15
		 * /	:	16
		 * (	:	31
		 * )	:	32
		 * S -> E | NA 
		 * E -> NE' | BE' | (E)E'
		 * E'-> FEE' | NA
		 * F-> + | - | * | /
		 * */

		class S : Non_TerminalSymbol
		{
			public S()
			{
				edge[11] = new int[] { -3 };
				edge[12] = new int[] { -3 };
				edge[13] = new int[] { -3 };
				edge[14] = new int[] { -3 };
				edge[15] = new int[] { -3 };
				edge[16] = new int[] { -3 };
				edge[31] = new int[] { -3 };
				edge[-1] = new int[] { 0 };
			}
		}
		class E : Non_TerminalSymbol
		{
			public E()
			{
				edge[11] = new int[] { 11, -4 };
				edge[12] = new int[] { 12, -4 };
				edge[31] = new int[] { 31, -3, 32, -4 };
			}
		}
		class Ex : Non_TerminalSymbol
		{
			public Ex()
			{
				edge[13] = new int[] { -5, -3, -4 };
				edge[14] = new int[] { -5, -3, -4 };
				edge[15] = new int[] { -5, -3, -4 };
				edge[16] = new int[] { -5, -3, -4 };
				edge[32] = new int[] { 0 };
				edge[-1] = new int[] { 0 };
			}
		}
		class F : Non_TerminalSymbol
		{
			public F()
			{
				edge[13] = new int[] { 13 };
				edge[14] = new int[] { 14 };
				edge[15] = new int[] { 15 };
				edge[16] = new int[] { 16 };
			}
		}
		public ParsingMachine(Token[] tokens)
		{
			this.tokens = tokens;
		}
		public bool Run()
		{
			Stack<SymbolInterface> stack = new Stack<SymbolInterface>();
			int position = 0;
			stack.Push(new TerminalSymbol(-1));
			stack.Push(new S());
			while(true)
			{
				if (stack.Count() == 0 || position == tokens.Length) break;
				if (stack.Peek() is Non_TerminalSymbol)
				{
					int[] firstSet = (stack.Pop() as Non_TerminalSymbol).First(tokens[position].code);
					if (firstSet == null)
					{
						Console.WriteLine(position);
						return false;
					}
						
					for (int i = firstSet.Length - 1; i >= 0; i--)
					{
						if (firstSet[i] == 0) continue;
						if (firstSet[i] <= -2)
							stack.Push(Non_TerminalSymbol.CreateNon_TerminalSymbol(firstSet[i]));
						else
							stack.Push(new TerminalSymbol(firstSet[i]));
					}
				}
				else if (stack.Peek() is TerminalSymbol)
				{
					TerminalSymbol item = stack.Pop() as TerminalSymbol;
					if (item.code == tokens[position].code)
					{
						position++;
					}
					else return false;
				}
				else return false;
			}
			if (stack.Count() == 0 && position == tokens.Length) return true;
			return false;
		}
	}
	class Token
	{
		public String content { get; set; }
		public int code { get; set; }
		public int line { get; set; }
		public Token(String content,int code,int line)
		{
			this.content = content;
			this.code = code;
			this.line = line;
		}
		
	}
}
