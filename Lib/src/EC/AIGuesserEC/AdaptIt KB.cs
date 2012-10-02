﻿//------------------------------------------------------------------------------
// <autogenerated>
//     This code was generated by a tool.
//     Runtime Version: 1.1.4322.2032
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </autogenerated>
//------------------------------------------------------------------------------

namespace SilEncConverters31 {
	using System;
	using System.Data;
	using System.Xml;
	using System.Runtime.Serialization;


	[Serializable()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.ToolboxItem(true)]
	public class AdaptItKnowledgeBase : DataSet
	{

		private KBDataTable tableKB;

		private MAPDataTable tableMAP;

		private TUDataTable tableTU;

		private RSDataTable tableRS;

		private DataRelation relationTU_RS;

		private DataRelation relationMAP_TU;

		private DataRelation relationKB_MAP;

		public AdaptItKnowledgeBase()
		{
			this.InitClass();
			System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
			this.Tables.CollectionChanged += schemaChangedHandler;
			this.Relations.CollectionChanged += schemaChangedHandler;
		}

		protected AdaptItKnowledgeBase(SerializationInfo info, StreamingContext context)
		{
			string strSchema = ((string)(info.GetValue("XmlSchema", typeof(string))));
			if ((strSchema != null))
			{
				DataSet ds = new DataSet();
				ds.ReadXmlSchema(new XmlTextReader(new System.IO.StringReader(strSchema)));
				if ((ds.Tables["KB"] != null))
				{
					this.Tables.Add(new KBDataTable(ds.Tables["KB"]));
				}
				if ((ds.Tables["MAP"] != null))
				{
					this.Tables.Add(new MAPDataTable(ds.Tables["MAP"]));
				}
				if ((ds.Tables["TU"] != null))
				{
					this.Tables.Add(new TUDataTable(ds.Tables["TU"]));
				}
				if ((ds.Tables["RS"] != null))
				{
					this.Tables.Add(new RSDataTable(ds.Tables["RS"]));
				}
				this.DataSetName = ds.DataSetName;
				this.Prefix = ds.Prefix;
				this.Namespace = ds.Namespace;
				this.Locale = ds.Locale;
				this.CaseSensitive = ds.CaseSensitive;
				this.EnforceConstraints = ds.EnforceConstraints;
				this.Merge(ds, false, System.Data.MissingSchemaAction.Add);
				this.InitVars();
			}
			else
			{
				this.InitClass();
			}
			this.GetSerializationData(info, context);
			System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
			this.Tables.CollectionChanged += schemaChangedHandler;
			this.Relations.CollectionChanged += schemaChangedHandler;
		}

		[System.ComponentModel.Browsable(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Content)]
		public KBDataTable KB
		{
			get
			{
				return this.tableKB;
			}
		}

		[System.ComponentModel.Browsable(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Content)]
		public MAPDataTable MAP
		{
			get
			{
				return this.tableMAP;
			}
		}

		[System.ComponentModel.Browsable(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Content)]
		public TUDataTable TU
		{
			get
			{
				return this.tableTU;
			}
		}

		[System.ComponentModel.Browsable(false)]
		[System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Content)]
		public RSDataTable RS
		{
			get
			{
				return this.tableRS;
			}
		}

		public override DataSet Clone()
		{
			AdaptItKnowledgeBase cln = ((AdaptItKnowledgeBase)(base.Clone()));
			cln.InitVars();
			return cln;
		}

		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		protected override void ReadXmlSerializable(XmlReader reader)
		{
			this.Reset();
			DataSet ds = new DataSet();
			ds.ReadXml(reader);
			if ((ds.Tables["KB"] != null))
			{
				this.Tables.Add(new KBDataTable(ds.Tables["KB"]));
			}
			if ((ds.Tables["MAP"] != null))
			{
				this.Tables.Add(new MAPDataTable(ds.Tables["MAP"]));
			}
			if ((ds.Tables["TU"] != null))
			{
				this.Tables.Add(new TUDataTable(ds.Tables["TU"]));
			}
			if ((ds.Tables["RS"] != null))
			{
				this.Tables.Add(new RSDataTable(ds.Tables["RS"]));
			}
			this.DataSetName = ds.DataSetName;
			this.Prefix = ds.Prefix;
			this.Namespace = ds.Namespace;
			this.Locale = ds.Locale;
			this.CaseSensitive = ds.CaseSensitive;
			this.EnforceConstraints = ds.EnforceConstraints;
			this.Merge(ds, false, System.Data.MissingSchemaAction.Add);
			this.InitVars();
		}

		protected override System.Xml.Schema.XmlSchema GetSchemaSerializable()
		{
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			this.WriteXmlSchema(new XmlTextWriter(stream, null));
			stream.Position = 0;
			return System.Xml.Schema.XmlSchema.Read(new XmlTextReader(stream), null);
		}

		internal void InitVars()
		{
			this.tableKB = ((KBDataTable)(this.Tables["KB"]));
			if ((this.tableKB != null))
			{
				this.tableKB.InitVars();
			}
			this.tableMAP = ((MAPDataTable)(this.Tables["MAP"]));
			if ((this.tableMAP != null))
			{
				this.tableMAP.InitVars();
			}
			this.tableTU = ((TUDataTable)(this.Tables["TU"]));
			if ((this.tableTU != null))
			{
				this.tableTU.InitVars();
			}
			this.tableRS = ((RSDataTable)(this.Tables["RS"]));
			if ((this.tableRS != null))
			{
				this.tableRS.InitVars();
			}
			this.relationTU_RS = this.Relations["TU_RS"];
			this.relationMAP_TU = this.Relations["MAP_TU"];
			this.relationKB_MAP = this.Relations["KB_MAP"];
		}

		private void InitClass()
		{
			this.DataSetName = "AdaptItKnowledgeBase";
			this.Prefix = "";
			this.Namespace = "http://www.sil.org/computing/schemas/AdaptIt KB.xsd";
			this.Locale = new System.Globalization.CultureInfo("en-US");
			this.CaseSensitive = false;
			this.EnforceConstraints = false;
			this.tableKB = new KBDataTable();
			this.Tables.Add(this.tableKB);
			this.tableMAP = new MAPDataTable();
			this.Tables.Add(this.tableMAP);
			this.tableTU = new TUDataTable();
			this.Tables.Add(this.tableTU);
			this.tableRS = new RSDataTable();
			this.Tables.Add(this.tableRS);
			ForeignKeyConstraint fkc;
			fkc = new ForeignKeyConstraint("KB_MAP", new DataColumn[] {
						this.tableKB.KB_IdColumn}, new DataColumn[] {
						this.tableMAP.KB_IdColumn});
			this.tableMAP.Constraints.Add(fkc);
			fkc.AcceptRejectRule = System.Data.AcceptRejectRule.None;
			fkc.DeleteRule = System.Data.Rule.Cascade;
			fkc.UpdateRule = System.Data.Rule.Cascade;
			fkc = new ForeignKeyConstraint("MAP_TU", new DataColumn[] {
						this.tableMAP.mnColumn}, new DataColumn[] {
						this.tableTU.mnColumn});
			this.tableTU.Constraints.Add(fkc);
			fkc.AcceptRejectRule = System.Data.AcceptRejectRule.None;
			fkc.DeleteRule = System.Data.Rule.Cascade;
			fkc.UpdateRule = System.Data.Rule.Cascade;
			fkc = new ForeignKeyConstraint("TU_RS", new DataColumn[] {
						this.tableTU.kColumn}, new DataColumn[] {
						this.tableRS.kColumn});
			this.tableRS.Constraints.Add(fkc);
			fkc.AcceptRejectRule = System.Data.AcceptRejectRule.None;
			fkc.DeleteRule = System.Data.Rule.Cascade;
			fkc.UpdateRule = System.Data.Rule.Cascade;
			this.relationTU_RS = new DataRelation("TU_RS", new DataColumn[] {
						this.tableTU.kColumn}, new DataColumn[] {
						this.tableRS.kColumn}, false);
			this.relationTU_RS.Nested = true;
			this.Relations.Add(this.relationTU_RS);
			this.relationMAP_TU = new DataRelation("MAP_TU", new DataColumn[] {
						this.tableMAP.mnColumn}, new DataColumn[] {
						this.tableTU.mnColumn}, false);
			this.relationMAP_TU.Nested = true;
			this.Relations.Add(this.relationMAP_TU);
			this.relationKB_MAP = new DataRelation("KB_MAP", new DataColumn[] {
						this.tableKB.KB_IdColumn}, new DataColumn[] {
						this.tableMAP.KB_IdColumn}, false);
			this.relationKB_MAP.Nested = true;
			this.Relations.Add(this.relationKB_MAP);
		}

		private bool ShouldSerializeKB()
		{
			return false;
		}

		private bool ShouldSerializeMAP()
		{
			return false;
		}

		private bool ShouldSerializeTU()
		{
			return false;
		}

		private bool ShouldSerializeRS()
		{
			return false;
		}

		private void SchemaChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
		{
			if ((e.Action == System.ComponentModel.CollectionChangeAction.Remove))
			{
				this.InitVars();
			}
		}

		public delegate void KBRowChangeEventHandler(object sender, KBRowChangeEvent e);

		public delegate void MAPRowChangeEventHandler(object sender, MAPRowChangeEvent e);

		public delegate void TURowChangeEventHandler(object sender, TURowChangeEvent e);

		public delegate void RSRowChangeEventHandler(object sender, RSRowChangeEvent e);

		[System.Diagnostics.DebuggerStepThrough()]
		public class KBDataTable : DataTable, System.Collections.IEnumerable
		{

			private DataColumn columndocVersion;

			private DataColumn columnsrcName;

			private DataColumn columntgtName;

			private DataColumn columnmax;

			private DataColumn columnKB_Id;

			internal KBDataTable()
				:
					base("KB")
			{
				this.InitClass();
			}

			internal KBDataTable(DataTable table)
				:
					base(table.TableName)
			{
				if ((table.CaseSensitive != table.DataSet.CaseSensitive))
				{
					this.CaseSensitive = table.CaseSensitive;
				}
				if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
				{
					this.Locale = table.Locale;
				}
				if ((table.Namespace != table.DataSet.Namespace))
				{
					this.Namespace = table.Namespace;
				}
				this.Prefix = table.Prefix;
				this.MinimumCapacity = table.MinimumCapacity;
				this.DisplayExpression = table.DisplayExpression;
			}

			[System.ComponentModel.Browsable(false)]
			public int Count
			{
				get
				{
					return this.Rows.Count;
				}
			}

			internal DataColumn docVersionColumn
			{
				get
				{
					return this.columndocVersion;
				}
			}

			internal DataColumn srcNameColumn
			{
				get
				{
					return this.columnsrcName;
				}
			}

			internal DataColumn tgtNameColumn
			{
				get
				{
					return this.columntgtName;
				}
			}

			internal DataColumn maxColumn
			{
				get
				{
					return this.columnmax;
				}
			}

			internal DataColumn KB_IdColumn
			{
				get
				{
					return this.columnKB_Id;
				}
			}

			public KBRow this[int index]
			{
				get
				{
					return ((KBRow)(this.Rows[index]));
				}
			}

			public event KBRowChangeEventHandler KBRowChanged;

			public event KBRowChangeEventHandler KBRowChanging;

			public event KBRowChangeEventHandler KBRowDeleted;

			public event KBRowChangeEventHandler KBRowDeleting;

			public void AddKBRow(KBRow row)
			{
				this.Rows.Add(row);
			}

			public KBRow AddKBRow(string docVersion, string srcName, string tgtName, string max)
			{
				KBRow rowKBRow = ((KBRow)(this.NewRow()));
				rowKBRow.ItemArray = new object[] {
						docVersion,
						srcName,
						tgtName,
						max,
						null};
				this.Rows.Add(rowKBRow);
				return rowKBRow;
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.Rows.GetEnumerator();
			}

			public override DataTable Clone()
			{
				KBDataTable cln = ((KBDataTable)(base.Clone()));
				cln.InitVars();
				return cln;
			}

			protected override DataTable CreateInstance()
			{
				return new KBDataTable();
			}

			internal void InitVars()
			{
				this.columndocVersion = this.Columns["docVersion"];
				this.columnsrcName = this.Columns["srcName"];
				this.columntgtName = this.Columns["tgtName"];
				this.columnmax = this.Columns["max"];
				this.columnKB_Id = this.Columns["KB_Id"];
			}

			private void InitClass()
			{
				this.columndocVersion = new DataColumn("docVersion", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columndocVersion);
				this.columnsrcName = new DataColumn("srcName", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columnsrcName);
				this.columntgtName = new DataColumn("tgtName", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columntgtName);
				this.columnmax = new DataColumn("max", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columnmax);
				this.columnKB_Id = new DataColumn("KB_Id", typeof(int), null, System.Data.MappingType.Hidden);
				this.Columns.Add(this.columnKB_Id);
				this.Constraints.Add(new UniqueConstraint("Constraint1", new DataColumn[] {
								this.columnKB_Id}, true));
				this.columndocVersion.Namespace = "";
				this.columnsrcName.Namespace = "";
				this.columntgtName.Namespace = "";
				this.columnmax.Namespace = "";
				this.columnKB_Id.AutoIncrement = true;
				this.columnKB_Id.AllowDBNull = false;
				this.columnKB_Id.Unique = true;
			}

			public KBRow NewKBRow()
			{
				return ((KBRow)(this.NewRow()));
			}

			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new KBRow(builder);
			}

			protected override System.Type GetRowType()
			{
				return typeof(KBRow);
			}

			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if ((this.KBRowChanged != null))
				{
					this.KBRowChanged(this, new KBRowChangeEvent(((KBRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if ((this.KBRowChanging != null))
				{
					this.KBRowChanging(this, new KBRowChangeEvent(((KBRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if ((this.KBRowDeleted != null))
				{
					this.KBRowDeleted(this, new KBRowChangeEvent(((KBRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if ((this.KBRowDeleting != null))
				{
					this.KBRowDeleting(this, new KBRowChangeEvent(((KBRow)(e.Row)), e.Action));
				}
			}

			public void RemoveKBRow(KBRow row)
			{
				this.Rows.Remove(row);
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class KBRow : DataRow
		{

			private KBDataTable tableKB;

			internal KBRow(DataRowBuilder rb)
				:
					base(rb)
			{
				this.tableKB = ((KBDataTable)(this.Table));
			}

			public string docVersion
			{
				get
				{
					try
					{
						return ((string)(this[this.tableKB.docVersionColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableKB.docVersionColumn] = value;
				}
			}

			public string srcName
			{
				get
				{
					try
					{
						return ((string)(this[this.tableKB.srcNameColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableKB.srcNameColumn] = value;
				}
			}

			public string tgtName
			{
				get
				{
					try
					{
						return ((string)(this[this.tableKB.tgtNameColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableKB.tgtNameColumn] = value;
				}
			}

			public string max
			{
				get
				{
					try
					{
						return ((string)(this[this.tableKB.maxColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableKB.maxColumn] = value;
				}
			}

			public bool IsdocVersionNull()
			{
				return this.IsNull(this.tableKB.docVersionColumn);
			}

			public void SetdocVersionNull()
			{
				this[this.tableKB.docVersionColumn] = System.Convert.DBNull;
			}

			public bool IssrcNameNull()
			{
				return this.IsNull(this.tableKB.srcNameColumn);
			}

			public void SetsrcNameNull()
			{
				this[this.tableKB.srcNameColumn] = System.Convert.DBNull;
			}

			public bool IstgtNameNull()
			{
				return this.IsNull(this.tableKB.tgtNameColumn);
			}

			public void SettgtNameNull()
			{
				this[this.tableKB.tgtNameColumn] = System.Convert.DBNull;
			}

			public bool IsmaxNull()
			{
				return this.IsNull(this.tableKB.maxColumn);
			}

			public void SetmaxNull()
			{
				this[this.tableKB.maxColumn] = System.Convert.DBNull;
			}

			public MAPRow[] GetMAPRows()
			{
				return ((MAPRow[])(this.GetChildRows(this.Table.ChildRelations["KB_MAP"])));
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class KBRowChangeEvent : EventArgs
		{

			private KBRow eventRow;

			private DataRowAction eventAction;

			public KBRowChangeEvent(KBRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			public KBRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class MAPDataTable : DataTable, System.Collections.IEnumerable
		{

			private DataColumn columnmn;

			private DataColumn columnKB_Id;

			internal MAPDataTable()
				:
					base("MAP")
			{
				this.InitClass();
			}

			internal MAPDataTable(DataTable table)
				:
					base(table.TableName)
			{
				if ((table.CaseSensitive != table.DataSet.CaseSensitive))
				{
					this.CaseSensitive = table.CaseSensitive;
				}
				if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
				{
					this.Locale = table.Locale;
				}
				if ((table.Namespace != table.DataSet.Namespace))
				{
					this.Namespace = table.Namespace;
				}
				this.Prefix = table.Prefix;
				this.MinimumCapacity = table.MinimumCapacity;
				this.DisplayExpression = table.DisplayExpression;
			}

			[System.ComponentModel.Browsable(false)]
			public int Count
			{
				get
				{
					return this.Rows.Count;
				}
			}

			internal DataColumn mnColumn
			{
				get
				{
					return this.columnmn;
				}
			}

			internal DataColumn KB_IdColumn
			{
				get
				{
					return this.columnKB_Id;
				}
			}

			public MAPRow this[int index]
			{
				get
				{
					return ((MAPRow)(this.Rows[index]));
				}
			}

			public event MAPRowChangeEventHandler MAPRowChanged;

			public event MAPRowChangeEventHandler MAPRowChanging;

			public event MAPRowChangeEventHandler MAPRowDeleted;

			public event MAPRowChangeEventHandler MAPRowDeleting;

			public void AddMAPRow(MAPRow row)
			{
				this.Rows.Add(row);
			}

			public MAPRow AddMAPRow(string mn, KBRow parentKBRowByKB_MAP)
			{
				MAPRow rowMAPRow = ((MAPRow)(this.NewRow()));
				rowMAPRow.ItemArray = new object[] {
						mn,
						parentKBRowByKB_MAP[4]};
				this.Rows.Add(rowMAPRow);
				return rowMAPRow;
			}

			public MAPRow FindBymn(string mn)
			{
				return ((MAPRow)(this.Rows.Find(new object[] {
							mn})));
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.Rows.GetEnumerator();
			}

			public override DataTable Clone()
			{
				MAPDataTable cln = ((MAPDataTable)(base.Clone()));
				cln.InitVars();
				return cln;
			}

			protected override DataTable CreateInstance()
			{
				return new MAPDataTable();
			}

			internal void InitVars()
			{
				this.columnmn = this.Columns["mn"];
				this.columnKB_Id = this.Columns["KB_Id"];
			}

			private void InitClass()
			{
				this.columnmn = new DataColumn("mn", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columnmn);
				this.columnKB_Id = new DataColumn("KB_Id", typeof(int), null, System.Data.MappingType.Hidden);
				this.Columns.Add(this.columnKB_Id);
				this.Constraints.Add(new UniqueConstraint("MNKey", new DataColumn[] {
								this.columnmn}, true));
				this.columnmn.AllowDBNull = false;
				this.columnmn.Unique = true;
				this.columnmn.Namespace = "";
			}

			public MAPRow NewMAPRow()
			{
				return ((MAPRow)(this.NewRow()));
			}

			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new MAPRow(builder);
			}

			protected override System.Type GetRowType()
			{
				return typeof(MAPRow);
			}

			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if ((this.MAPRowChanged != null))
				{
					this.MAPRowChanged(this, new MAPRowChangeEvent(((MAPRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if ((this.MAPRowChanging != null))
				{
					this.MAPRowChanging(this, new MAPRowChangeEvent(((MAPRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if ((this.MAPRowDeleted != null))
				{
					this.MAPRowDeleted(this, new MAPRowChangeEvent(((MAPRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if ((this.MAPRowDeleting != null))
				{
					this.MAPRowDeleting(this, new MAPRowChangeEvent(((MAPRow)(e.Row)), e.Action));
				}
			}

			public void RemoveMAPRow(MAPRow row)
			{
				this.Rows.Remove(row);
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class MAPRow : DataRow
		{

			private MAPDataTable tableMAP;

			internal MAPRow(DataRowBuilder rb)
				:
					base(rb)
			{
				this.tableMAP = ((MAPDataTable)(this.Table));
			}

			public string mn
			{
				get
				{
					return ((string)(this[this.tableMAP.mnColumn]));
				}
				set
				{
					this[this.tableMAP.mnColumn] = value;
				}
			}

			public KBRow KBRow
			{
				get
				{
					return ((KBRow)(this.GetParentRow(this.Table.ParentRelations["KB_MAP"])));
				}
				set
				{
					this.SetParentRow(value, this.Table.ParentRelations["KB_MAP"]);
				}
			}

			public TURow[] GetTURows()
			{
				return ((TURow[])(this.GetChildRows(this.Table.ChildRelations["MAP_TU"])));
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class MAPRowChangeEvent : EventArgs
		{

			private MAPRow eventRow;

			private DataRowAction eventAction;

			public MAPRowChangeEvent(MAPRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			public MAPRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class TUDataTable : DataTable, System.Collections.IEnumerable
		{

			private DataColumn columnf;

			private DataColumn columnk;

			private DataColumn columnmn;

			internal TUDataTable()
				:
					base("TU")
			{
				this.InitClass();
			}

			internal TUDataTable(DataTable table)
				:
					base(table.TableName)
			{
				if ((table.CaseSensitive != table.DataSet.CaseSensitive))
				{
					this.CaseSensitive = table.CaseSensitive;
				}
				if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
				{
					this.Locale = table.Locale;
				}
				if ((table.Namespace != table.DataSet.Namespace))
				{
					this.Namespace = table.Namespace;
				}
				this.Prefix = table.Prefix;
				this.MinimumCapacity = table.MinimumCapacity;
				this.DisplayExpression = table.DisplayExpression;
			}

			[System.ComponentModel.Browsable(false)]
			public int Count
			{
				get
				{
					return this.Rows.Count;
				}
			}

			internal DataColumn fColumn
			{
				get
				{
					return this.columnf;
				}
			}

			internal DataColumn kColumn
			{
				get
				{
					return this.columnk;
				}
			}

			internal DataColumn mnColumn
			{
				get
				{
					return this.columnmn;
				}
			}

			public TURow this[int index]
			{
				get
				{
					return ((TURow)(this.Rows[index]));
				}
			}

			public event TURowChangeEventHandler TURowChanged;

			public event TURowChangeEventHandler TURowChanging;

			public event TURowChangeEventHandler TURowDeleted;

			public event TURowChangeEventHandler TURowDeleting;

			public void AddTURow(TURow row)
			{
				this.Rows.Add(row);
			}

			public TURow AddTURow(string f, string k, MAPRow parentMAPRowByMAP_TU)
			{
				TURow rowTURow = ((TURow)(this.NewRow()));
				rowTURow.ItemArray = new object[] {
						f,
						k,
						parentMAPRowByMAP_TU[0]};
				this.Rows.Add(rowTURow);
				return rowTURow;
			}

			public TURow FindByk(string k)
			{
				return ((TURow)(this.Rows.Find(new object[] {
							k})));
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.Rows.GetEnumerator();
			}

			public override DataTable Clone()
			{
				TUDataTable cln = ((TUDataTable)(base.Clone()));
				cln.InitVars();
				return cln;
			}

			protected override DataTable CreateInstance()
			{
				return new TUDataTable();
			}

			internal void InitVars()
			{
				this.columnf = this.Columns["f"];
				this.columnk = this.Columns["k"];
				this.columnmn = this.Columns["mn"];
			}

			private void InitClass()
			{
				this.columnf = new DataColumn("f", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columnf);
				this.columnk = new DataColumn("k", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columnk);
				this.columnmn = new DataColumn("mn", typeof(string), null, System.Data.MappingType.Hidden);
				this.Columns.Add(this.columnmn);
				this.Constraints.Add(new UniqueConstraint("KKey", new DataColumn[] {
								this.columnk}, true));
				this.columnf.Namespace = "";
				this.columnk.AllowDBNull = false;
				this.columnk.Unique = true;
				this.columnk.Namespace = "";
			}

			public TURow NewTURow()
			{
				return ((TURow)(this.NewRow()));
			}

			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new TURow(builder);
			}

			protected override System.Type GetRowType()
			{
				return typeof(TURow);
			}

			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if ((this.TURowChanged != null))
				{
					this.TURowChanged(this, new TURowChangeEvent(((TURow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if ((this.TURowChanging != null))
				{
					this.TURowChanging(this, new TURowChangeEvent(((TURow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if ((this.TURowDeleted != null))
				{
					this.TURowDeleted(this, new TURowChangeEvent(((TURow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if ((this.TURowDeleting != null))
				{
					this.TURowDeleting(this, new TURowChangeEvent(((TURow)(e.Row)), e.Action));
				}
			}

			public void RemoveTURow(TURow row)
			{
				this.Rows.Remove(row);
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class TURow : DataRow
		{

			private TUDataTable tableTU;

			internal TURow(DataRowBuilder rb)
				:
					base(rb)
			{
				this.tableTU = ((TUDataTable)(this.Table));
			}

			public string f
			{
				get
				{
					try
					{
						return ((string)(this[this.tableTU.fColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableTU.fColumn] = value;
				}
			}

			public string k
			{
				get
				{
					return ((string)(this[this.tableTU.kColumn]));
				}
				set
				{
					this[this.tableTU.kColumn] = value;
				}
			}

			public MAPRow MAPRow
			{
				get
				{
					return ((MAPRow)(this.GetParentRow(this.Table.ParentRelations["MAP_TU"])));
				}
				set
				{
					this.SetParentRow(value, this.Table.ParentRelations["MAP_TU"]);
				}
			}

			public bool IsfNull()
			{
				return this.IsNull(this.tableTU.fColumn);
			}

			public void SetfNull()
			{
				this[this.tableTU.fColumn] = System.Convert.DBNull;
			}

			public RSRow[] GetRSRows()
			{
				return ((RSRow[])(this.GetChildRows(this.Table.ChildRelations["TU_RS"])));
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class TURowChangeEvent : EventArgs
		{

			private TURow eventRow;

			private DataRowAction eventAction;

			public TURowChangeEvent(TURow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			public TURow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class RSDataTable : DataTable, System.Collections.IEnumerable
		{

			private DataColumn columnn;

			private DataColumn columna;

			private DataColumn columnk;

			internal RSDataTable()
				:
					base("RS")
			{
				this.InitClass();
			}

			internal RSDataTable(DataTable table)
				:
					base(table.TableName)
			{
				if ((table.CaseSensitive != table.DataSet.CaseSensitive))
				{
					this.CaseSensitive = table.CaseSensitive;
				}
				if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
				{
					this.Locale = table.Locale;
				}
				if ((table.Namespace != table.DataSet.Namespace))
				{
					this.Namespace = table.Namespace;
				}
				this.Prefix = table.Prefix;
				this.MinimumCapacity = table.MinimumCapacity;
				this.DisplayExpression = table.DisplayExpression;
			}

			[System.ComponentModel.Browsable(false)]
			public int Count
			{
				get
				{
					return this.Rows.Count;
				}
			}

			internal DataColumn nColumn
			{
				get
				{
					return this.columnn;
				}
			}

			internal DataColumn aColumn
			{
				get
				{
					return this.columna;
				}
			}

			internal DataColumn kColumn
			{
				get
				{
					return this.columnk;
				}
			}

			public RSRow this[int index]
			{
				get
				{
					return ((RSRow)(this.Rows[index]));
				}
			}

			public event RSRowChangeEventHandler RSRowChanged;

			public event RSRowChangeEventHandler RSRowChanging;

			public event RSRowChangeEventHandler RSRowDeleted;

			public event RSRowChangeEventHandler RSRowDeleting;

			public void AddRSRow(RSRow row)
			{
				this.Rows.Add(row);
			}

			public RSRow AddRSRow(string n, string a, TURow parentTURowByTU_RS)
			{
				RSRow rowRSRow = ((RSRow)(this.NewRow()));
				rowRSRow.ItemArray = new object[] {
						n,
						a,
						parentTURowByTU_RS[1]};
				this.Rows.Add(rowRSRow);
				return rowRSRow;
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.Rows.GetEnumerator();
			}

			public override DataTable Clone()
			{
				RSDataTable cln = ((RSDataTable)(base.Clone()));
				cln.InitVars();
				return cln;
			}

			protected override DataTable CreateInstance()
			{
				return new RSDataTable();
			}

			internal void InitVars()
			{
				this.columnn = this.Columns["n"];
				this.columna = this.Columns["a"];
				this.columnk = this.Columns["k"];
			}

			private void InitClass()
			{
				this.columnn = new DataColumn("n", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columnn);
				this.columna = new DataColumn("a", typeof(string), null, System.Data.MappingType.Attribute);
				this.Columns.Add(this.columna);
				this.columnk = new DataColumn("k", typeof(string), null, System.Data.MappingType.Hidden);
				this.Columns.Add(this.columnk);
				this.columnn.Namespace = "";
				this.columna.Namespace = "";
			}

			public RSRow NewRSRow()
			{
				return ((RSRow)(this.NewRow()));
			}

			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new RSRow(builder);
			}

			protected override System.Type GetRowType()
			{
				return typeof(RSRow);
			}

			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if ((this.RSRowChanged != null))
				{
					this.RSRowChanged(this, new RSRowChangeEvent(((RSRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if ((this.RSRowChanging != null))
				{
					this.RSRowChanging(this, new RSRowChangeEvent(((RSRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if ((this.RSRowDeleted != null))
				{
					this.RSRowDeleted(this, new RSRowChangeEvent(((RSRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if ((this.RSRowDeleting != null))
				{
					this.RSRowDeleting(this, new RSRowChangeEvent(((RSRow)(e.Row)), e.Action));
				}
			}

			public void RemoveRSRow(RSRow row)
			{
				this.Rows.Remove(row);
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class RSRow : DataRow
		{

			private RSDataTable tableRS;

			internal RSRow(DataRowBuilder rb)
				:
					base(rb)
			{
				this.tableRS = ((RSDataTable)(this.Table));
			}

			public string n
			{
				get
				{
					try
					{
						return ((string)(this[this.tableRS.nColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableRS.nColumn] = value;
				}
			}

			public string a
			{
				get
				{
					try
					{
						return ((string)(this[this.tableRS.aColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableRS.aColumn] = value;
				}
			}

			public TURow TURow
			{
				get
				{
					return ((TURow)(this.GetParentRow(this.Table.ParentRelations["TU_RS"])));
				}
				set
				{
					this.SetParentRow(value, this.Table.ParentRelations["TU_RS"]);
				}
			}

			public bool IsnNull()
			{
				return this.IsNull(this.tableRS.nColumn);
			}

			public void SetnNull()
			{
				this[this.tableRS.nColumn] = System.Convert.DBNull;
			}

			public bool IsaNull()
			{
				return this.IsNull(this.tableRS.aColumn);
			}

			public void SetaNull()
			{
				this[this.tableRS.aColumn] = System.Convert.DBNull;
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class RSRowChangeEvent : EventArgs
		{

			private RSRow eventRow;

			private DataRowAction eventAction;

			public RSRowChangeEvent(RSRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			public RSRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}
		}
	}
}
