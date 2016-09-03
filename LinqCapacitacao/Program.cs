using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GLib;

namespace LinqCapacitacao
{
	class Pessoa
	{
		public string Nome { get; set;}
		public int Idade { get; set;}
		public DateTime DataNascimento { get; set; }
	}

	interface IRepository<TEntity>
	{
		void Save(TEntity entity, params Func<TEntity, object>[] keys);
		void Delete(Func<TEntity, bool> condition);
		IEnumerable<TEntity> Find(Func<TEntity, bool> conditio);
		TEntity FindById (Func<TEntity, bool> condition);
		IEnumerable<TEntity> ToList();
	}

	interface IDao<TEntity>
	{
		void Create (TEntity entity);
		void Update (TEntity entity);
		void Delete (TEntity entity);
		IEnumerable<TEntity> List ();
	}

	abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
	{
		private readonly IDao<TEntity> _dao;

		public Repository (IDao<TEntity> dao)
		{
			_dao = dao;	
		}

		#region IRepository implementation

		public void Save (TEntity entity, params Func<TEntity, object>[] keys)
		{
			var result = true;
			foreach (var item in keys) {
				if (entity.GetType ().GetProperty (item.GetType ().Name) == null)
					result = false;
			}
			if (result)
				_dao.Create (entity);
			else
				_dao.Update (entity);
		}

		public void Delete (Func<TEntity, bool> condition)
		{
			var list = _dao.List ().ToList();
			var toremove = new List<TEntity>();

			if (list.Any (condition))
				toremove = list.Where (condition).ToList();
			if (toremove.Count > 0) 
				toremove.ToList().ForEach(c => list.Remove(c));
		}

		public IEnumerable<TEntity> Find (Func<TEntity, bool> condition)
		{
			return _dao.List ().Where (condition);
		}

		public TEntity FindById (Func<TEntity, bool> condition)
		{
			var list = _dao.List ();

			if (list.Any (condition))
				return list.FirstOrDefault (condition);
			else
				return null;
		}

		public IEnumerable<TEntity> ToList ()
		{
			return _dao.List ();
		}

		#endregion
	}

	delegate TResult Test<TEntity, TResult>(TEntity entity);



	class MainClass
	{
		public static void Main (string[] args)
		{
			DeletageEFunc ();
			LinqELambda ();
			Teste (delegate(int entity) { return entity % 2 == 0; });
			Teste (c => c == 4);
			Teste (Method);
			TesteExpression ();
			TesteIEnumerable ();
			TesteList ();
			TesteOrderBy ();
			TesteSum ();
			TesteSelect ();
		}

		public static void TesteSelect()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Select");

			var lista = new[] 
			{
				new { Nome = "Marvin Thomaz do Nascimento", Idade = 25 },
				new { Nome = "Claudia Denise Thomaz dos Santos Nascimento", Idade = 48 },
				new { Nome = "Jorge Batista do Nascimento", Idade = 63 },
				new { Nome = "Giovanna Thomaz do Nascimento", Idade = 13 }
			};

			foreach (var item in lista.SelectMany(c => c.Nome.Replace(" do ", " ").Replace(" dos ", " ").Split(' '))) {
				Console.WriteLine(item);
			}

			foreach (var item in lista.Select(c => new { Nome = c.Nome.Split(' ')[0], Sobrenome = c.Nome.Replace(" do ", " ").Replace(" dos ", " ").Split(' ')[1] })) {
				Console.WriteLine(item.Nome + " " + item.Sobrenome);
			}
		}

		public static void TesteSum()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Fold and Sum");

			var lista = new[] { 1, 2, 3, 4, 5, 6, 7 };

			Console.WriteLine ("Soma: " + lista.Sum ()); 
			Console.WriteLine ("Média: " + lista.Average ());
			Console.WriteLine ("Mínimo: " + lista.Min ());
			Console.WriteLine ("Máximo: " + lista.Max ());
		}

		public static void TesteOrderBy()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Order By");

			var lista = new []
			{ 
				new { Nome = "Marvin", Idade = 25, DataNascimento = new DateTime(1990, 12, 27) },
				new { Nome = "Cláudia", Idade = 49, DataNascimento = new DateTime(1968, 04, 15) },
				new { Nome = "Cláudia", Idade = 48, DataNascimento = new DateTime(1968, 04, 15) },
				new { Nome = "Giovanna", Idade = 13, DataNascimento = new DateTime(2003, 07, 04) },
				new { Nome = "Jorge", Idade = 63, DataNascimento = new DateTime(1953, 04, 02) }
			};

			lista = lista.OrderBy (c => c.Nome).ThenBy(c => c.Idade).Reverse().ToArray();

			foreach (var item in lista) {
				Console.WriteLine (item.Nome + " " + item.Idade + " " + item.DataNascimento.ToShortDateString());
			}

			foreach (var item in lista.GroupBy(c => c.Nome)) {
				Console.WriteLine ("Grupo: " + item.Key);
				Console.WriteLine ("-----//-----");

				foreach (var subitem in item) {
					Console.WriteLine (subitem.Nome + " " + subitem.Idade + " " + subitem.DataNascimento.ToShortDateString());
				}
			}

			foreach (var item in lista.GroupBy(c => c.Nome, c => c.Nome + " " + c.Idade + " " + c.DataNascimento.ToShortDateString())) {
				Console.WriteLine ("Grupo: " + item.Key);
				Console.WriteLine ("-----//-----");

				foreach (var subitem in item) {
					Console.WriteLine (subitem);
				}
			}
		}

		public static void TesteIEnumerable()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			// declare a variable containing some strings
			string[] names = { "Allen", "Arthur", "Bennett" };
			// declare a variable that represents a query
			IEnumerable<string> ayes = names.Where(s => s[0] == 'A');
			// evaluate the query
			foreach (string item in ayes)
				Console.WriteLine(item);
			// modify the original information source
			names[0] = "Bob";
			// evaluate the query again, this time no "Allen"
			foreach (string item in ayes)
				Console.WriteLine(item);
		}

		public static void TesteList()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			// declare a variable containing some strings
			string[] names = { "Allen", "Arthur", "Bennett" };
			// declare a variable that represents a query
			var ayes = names.Where(s => s[0] == 'A').ToList();
			// evaluate the query
			foreach (string item in ayes)
				Console.WriteLine(item);
			// modify the original information source
			names[0] = "Bob";
			// evaluate the query again, this time no "Allen"
			foreach (string item in ayes)
				Console.WriteLine(item);
		}

		public static void TesteExpression()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Expression");

			Expression<Func<Pessoa, bool>> inteiro = c => c.Idade == 25;

			var teste = inteiro.ToString ();

			Console.WriteLine (teste);

			teste = inteiro.Body.ToString ();

			Console.WriteLine (teste);

			teste = inteiro.Body.NodeType.ToString();

			Console.WriteLine (teste);

			//BinaryExpression binary = (BinaryExpression)inteiro.Body;

			//Console.WriteLine ("Campo: {0}", ((ParameterExpression)binary.Left).Name);
		}

		public static bool Method(int integer)
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			return integer * 2 == 10;
		}

		public static void Teste(Test<int, bool> meudelegate)
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Método Anônimo");
			Console.WriteLine (meudelegate (4));
		}

		public static void DeletageEFunc()
		{
			Console.WriteLine ("");
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Delegates and Functions");

			Func<string, bool> filter = delegate (string teste) {
				return teste.Length == 6;
			};

			Console.WriteLine(filter("Marvin"));
		}

		public static void LinqELambda()
		{
			Console.WriteLine ("-----//-----");
			Console.WriteLine ("Linq and Lambda");

			string[] lista = {"Marvin", "Rafaela", "Giovanna", "Jorge", "Claudia", "Tom", "Suze"};

			var resultado =  from item in lista where item.Length < 6 orderby item select item.ToUpper();
			var resultadolambda = lista.Where (c => c.Length < 6).OrderBy (c => c).Select (c => c.ToUpper ());

			foreach (var item in resultado) {
				Console.WriteLine (item);
			}

			Console.WriteLine ("");

			foreach (var item in resultadolambda) {
				Console.WriteLine (item);
			}
		}
	}
}
