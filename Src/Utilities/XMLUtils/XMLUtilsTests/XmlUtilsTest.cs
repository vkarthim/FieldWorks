// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2003, SIL International. All Rights Reserved.
// <copyright from='2003' to='2003' company='SIL International'>
//		Copyright (c) 2003, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: XmlUtilsTest.cs
// Responsibility: Andy Black
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Xml;
using System.Text;
using NUnit.Framework;
using SIL.FieldWorks.Common.Utils;

namespace SIL.Utils
{
	/// <summary>
	/// Summary description for XmlUtilsTest.
	/// </summary>
	[TestFixture]
	public class XmlUtilsTest
	{
		/// <summary>
		/// Location of test files
		/// </summary>
		protected string m_sTestPath = Path.Combine(SIL.FieldWorks.Common.Utils.DirectoryFinder.FwSourceDirectory,
			@"Utilities\XMLUtils\XMLUtilsTests\TransformTestFiles");

		/// <summary>
		/// Constructor.
		/// </summary>
		public XmlUtilsTest()
		{
		}

		/// <summary>
		///
		/// </summary>
		[Test]
		public void GetBooleanAttributeValueTest()
		{
			// All in this section should return false.
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue(null), "Null returns false");
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue("FALSE"), "'FALSE' returns false");
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue("False"), "'False' returns false");
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue("false"), "'false' returns false");
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue("NO"), "'NO' returns false");
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue("No"), "'No' returns false");
			Assert.IsFalse(XmlUtils.GetBooleanAttributeValue("no"), "'no' returns false");

			// All in this section should return true.
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue("TRUE"), "'TRUE' returns true");
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue("True"), "'True' returns true");
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue("true"), "'true' returns true");
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue("YES"), "'YES' returns true");
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue("Yes"), "'Yes' returns true");
			Assert.IsTrue(XmlUtils.GetBooleanAttributeValue("yes"), "'yes' returns true");
		}
		[Test]
		public void TransformFileToFileWithParametersTest()
		{
			// set up transform (use local copy so don't have to keep it in sync with the real one)
			string sTransform = Path.Combine(m_sTestPath, "Test.xsl");
			// build parameter list
			XmlUtils.XSLParameter[] parameterList = new XmlUtils.XSLParameter[2];
			parameterList[0] = new XmlUtils.XSLParameter("prmiNumber", "10");
			parameterList[1] = new XmlUtils.XSLParameter("prmsNumber", "ten");
			// use local input file
			string sInput = Path.Combine(m_sTestPath, "Test.xml");
			// create temp output file
			string sResult = FwTempFile.CreateTempFileAndGetPath("xml");
			// do transform
			SIL.Utils.XmlUtils.TransformFileToFile(sTransform, parameterList, sInput, sResult);
			// check results
			string sExpected;
			if (SIL.Utils.XmlUtils.UsingDotNetTransforms())
				sExpected = "ResultWithParams.xml";
			else
				sExpected = "ResultWithParamsMSXML2.xml";
			CheckXmlEquals(Path.Combine(m_sTestPath, sExpected), sResult);
			if (File.Exists(sResult))
				File.Delete(sResult);
		}
		[Test]
		public void TransformFileToFileNoParametersTest()
		{
			// set up transform (use local copy so don't have to keep it in sync with the real one)
			string sTransform = Path.Combine(m_sTestPath, "Test.xsl");
			// use local input file
			string sInput = Path.Combine(m_sTestPath, "Test.xml");
			// create temp output file
			string sResult = FwTempFile.CreateTempFileAndGetPath("xml");
			// do transform
			SIL.Utils.XmlUtils.TransformFileToFile(sTransform, sInput, sResult);
			// check results
			string sExpected;
			if (SIL.Utils.XmlUtils.UsingDotNetTransforms())
				sExpected = "ResultNoParams.xml";
			else
				sExpected = "ResultNoParamsMSXML2.xml";
			CheckXmlEquals(Path.Combine(m_sTestPath, sExpected), sResult);
			if (File.Exists(sResult))
				File.Delete(sResult);
		}
		private void CheckXmlEquals(string sExpectedResultFile, string sActualResultFile)
		{
			StreamReader expected = new StreamReader(sExpectedResultFile);
			StreamReader actual = new StreamReader(sActualResultFile);
			string sExpected = expected.ReadToEnd();
			string sActual = actual.ReadToEnd();
			StringBuilder sb = new StringBuilder();
			sb.Append("Expected file was ");
			sb.Append(sExpectedResultFile);
			Assert.AreEqual(sExpected, sActual, sb.ToString());
			expected.Close();
			actual.Close();
		}

		[Test]
		public void MakeSafeXmlAttributeTest()
		{
			string sFixed = XmlUtils.MakeSafeXmlAttribute("abc&def<ghi>jkl\"mno'pqr&stu");
			Assert.AreEqual("abc&amp;def&lt;ghi&gt;jkl&quot;mno&apos;pqr&amp;stu", sFixed, "First Test of MakeSafeXmlAttribute");

			sFixed = XmlUtils.MakeSafeXmlAttribute("abc&def\r\nghi\u001Fjkl\u007F\u009Fmno");
			Assert.AreEqual("abc&amp;def&#xD;&#xA;ghi&#x1F;jkl&#x7F;&#x9F;mno", sFixed, "Second Test of MakeSafeXmlAttribute");
		}

		[Test]
		public void DecodeXmlAttributeTest()
		{
			string sFixed = XmlUtils.DecodeXmlAttribute("abc&amp;def&lt;ghi&gt;jkl&quot;mno&apos;pqr&amp;stu");
			Assert.AreEqual("abc&def<ghi>jkl\"mno'pqr&stu", sFixed, "First Test of DecodeXmlAttribute");

			sFixed = XmlUtils.DecodeXmlAttribute("abc&amp;def&#xD;&#xA;ghi&#x1F;jkl&#x7F;&#x9F;mno");
			Assert.AreEqual("abc&def\r\nghi\u001Fjkl\u007F\u009Fmno", sFixed, "Second Test of DecodeXmlAttribute");
		}
	}
}