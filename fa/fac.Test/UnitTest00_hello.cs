using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace fac.Test {
	[TestClass]
	public class UnitTest00_hello {
		/// <summary>
		/// ���Hello
		/// </summary>
		[TestMethod]
		public void TestMethod1 () {
			string _code = @"
use fa;

class Program {
	public static void Main () {
		Console.Write (""hello"");
	}
}
";
			string _ret = BuildTool.RunAndGetReturn (_code);
			Assert.AreEqual (_ret, "hello");
		}

		/// <summary>
		/// �����ڽ�
		/// </summary>
		[TestMethod]
		public void TestMethod2 () {
			// ��ע�������ڽ�������μ� fa/fac/ASTs/Exprs/Names/AstExprName_BuildIn.cs
			string _code = @"
use fa;

class Program {
	public static void Main () {
		// ����ǰ�ļ�
		string _src1 = File.ReadAllText (@FILE);
		// ֱ�ӻ�ȡ��ǰ�ļ�Դ��
		string _src2 = @FILEDATA;
		if _src1 != _src2 {
			Console.WriteLine (""error"");
		}
		// ����̨���/����̨�����
		Console.Write (""a"");
		Console.WriteLine (""b"");
		// ����ļ��Ƿ����
		if !File.Exists (@FILE) {
			Console.WriteLine (""error"");
		}
		// д�ļ�
		File.WriteAllText (""D:\\a.fa"", ""// this file generate by fa test\r\n"");
		// ׷���ļ�
		File.AppendAllText (""D:\\a.fa"", @FILEDATA);
		// ����ļ���С
		string _src3 = File.ReadAllText (""D:\\a.fa"");
		Console.Write (""{0}"".Format (_src3.Length));
		// ɾ����ʱ�ļ�
		File.Delete (""D:\\a.fa"");
		//// �ж��ļ����Ƿ����
		//bool _b = Directory.Exists (""D:\\folder"");
		//// �����ļ���
		//Directory.Create (""D:\\folder"");
		//// ��ȡ�ļ����������ļ�����
		//string[] _files = Directory.GetFiles (""D:\\folder"");
		//// ɾ���ļ���
		//Directory.Delete (""D:\\folder"");
	}
}
";
			string _ret = BuildTool.RunAndGetReturn (_code);
			Assert.AreEqual (_ret[..4], "ab\r\n");
			Assert.IsTrue (int.Parse (_ret[4..]) > 500);
		}

		/// <summary>
		/// ����
		/// </summary>
		[TestMethod]
		public void TestMethod3 () {
			string _code = @"
use fa;

class Program {
	public static void Main () {
		int? n = null
		Console.Write (""{0} "".Format (n ?? 123))
		n = 100
		Console.Write (""{0}"".Format (n ?? 124))
	}
}
";
			string _ret = BuildTool.RunAndGetReturn (_code);
			Assert.AreEqual (_ret, "123 100");
		}
	}
}
