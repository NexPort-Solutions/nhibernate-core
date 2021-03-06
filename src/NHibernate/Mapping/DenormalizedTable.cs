using System;
using NHibernate.Util;
using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Mapping
{
	[Serializable]
	public class DenormalizedTable : Table
	{
		private readonly Table includedTable;

		public DenormalizedTable(Table includedTable)
		{
			this.includedTable = includedTable;
			includedTable.SetHasDenormalizedTables();
		}

		public override IEnumerable<Column> ColumnIterator
		{
			get { return includedTable.ColumnIterator.Concat(base.ColumnIterator); }
		}

		public override IEnumerable<UniqueKey> UniqueKeyIterator
		{
			get
			{
				//weird implementation because of hacky behavior
				//of Table.SqlCreateString() which modifies the
				//list of unique keys by side-effect on some dialects
				Dictionary<string, UniqueKey> uks = new Dictionary<string, UniqueKey>();
				ArrayHelper.AddAll(uks, UniqueKeys);
				ArrayHelper.AddAll(uks, includedTable.UniqueKeys);
				return uks.Values;
			}
		}

		public override IEnumerable<Index> IndexIterator
		{
			get
			{
				IEnumerable<Index> includedIdxs = includedTable.IndexIterator;
				foreach (Index parentIndex in includedIdxs)
				{
					var sharedIndex = GetIndex(parentIndex.Name);
					if (sharedIndex != null)
					{
						sharedIndex.AddColumns(parentIndex.ColumnIterator);
						sharedIndex.IsInherited = true;
						continue;
					}
					Index index = new Index();
					index.Name = parentIndex.Name;
					index.IsInherited = true;
					index.Table = this;
					index.AddColumns(parentIndex.ColumnIterator);
					yield return index;
				}

				foreach (var index in base.IndexIterator)
				{
					yield return index;
				}
			}
		}

		public override void CreateForeignKeys()
		{
			includedTable.CreateForeignKeys();
			var includedFks = includedTable.ForeignKeyIterator;
			foreach (var fk in includedFks)
			{
				CreateForeignKey(
					Constraint.GenerateName(
						fk.GeneratedConstraintNamePrefix,
						this,
						null,
						fk.Columns),
					fk.Columns, fk.ReferencedEntityName);
			}
		}

		public override Column GetColumn(Column column)
		{
			Column superColumn = base.GetColumn(column);
			if (superColumn != null)
			{
				return superColumn;
			}
			else
			{
				return includedTable.GetColumn(column);
			}
		}

		public override bool ContainsColumn(Column column)
		{
			return base.ContainsColumn(column) || includedTable.ContainsColumn(column);
		}

		public override PrimaryKey PrimaryKey
		{
			get
			{
				return includedTable.PrimaryKey;
			}
			set
			{
				base.PrimaryKey = value;
			}
		}
	}
}
