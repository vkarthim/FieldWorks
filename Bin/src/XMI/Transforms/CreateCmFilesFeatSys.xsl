<?xml version="1.0" encoding="UTF-8"?>
<!--This transform will output featsys.cmt from xmi2cellar3.xml in the XMITempOutputs directory.-->
<!--History
April 9, 2002 L. Hayashi - Added mod attributes to indicate the CellarModule where "referenced" classes come from-->
<!--&#xA; = newline-->
<xsl:stylesheet version="1.1" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
   <xsl:output method="xml" version="1.0" encoding="UTF-8" doctype-system="../../../bin/CellarModule.dtd"/>
   <xsl:text>&#xA;</xsl:text>
   <xsl:template match="/">
	  <xsl:apply-templates select="EntireModel/CellarModule[@id='FeatSys']"/>
   </xsl:template>
   <xsl:template match="CellarModule">
	  <xsl:text>&#xA;</xsl:text>
	  <xsl:comment>This is a temp cm file generated from \bin\src\xmi\FieldWorks.xmi. Do NOT edit this file directly!!!</xsl:comment>
	  <xsl:text>&#xA;</xsl:text>
	  <xsl:comment>Contact Larry Hayashi or Ken Zook to make a change.</xsl:comment>
	  <xsl:comment>This file is generated from Bin\Src\XMI\FieldWorks.xmi. DO NOT EDIT IT!!!</xsl:comment>
	  <xsl:text>&#xA;</xsl:text>
	  <xsl:variable name="CellarModuleName" select="@id"/>
	  <CellarModule>
		 <xsl:copy-of select="@*"/>
		 <xsl:text>&#xA;</xsl:text>
		 <!--This is stupid and a major pain and I'm sure there is an easier way...
			but because we are dumping each of these cm files separately, we must account for every signature class
			that is not in the file inorder for signatures, which are referred to, to validate.-->
		 <class num="0" mod="Cellar" id="CmObject" abbr="cmo" abstract="true" base="CmObject" depth="0"/>
		 <xsl:text>&#xA;</xsl:text>
		 <!--xsl:variable name="unique-signatures" select="class/props/*[not(@sig=preceding::*/@sig)]/@sig"/-->
		 <!--class/props/*[not(@sig=preceding::*/@sig)]/@sig-->
		 <!--xsl:value-of select="$unique-signatures"/-->
		 <xsl:for-each select="class">
			<xsl:variable name="classID" select="@id"/>
			<xsl:for-each select="props/*">
			   <xsl:variable name="propID" select="@id"/>
			   <xsl:variable name="PotentialPhonyClass" select="@sig"/>
			   <xsl:if test="@sig!='CmObject'">
				  <xsl:if test="name(.) !='basic'">
					 <!--xsl:variable name="PreviousNumber" select="(count(./preceding::*[@sig=$PotentialPhonyClass]))"/-->
					 <!--Check to see if there are any previous properties in this class with the same signature (if there is, then this dummy class has already been created-->
					 <xsl:variable name="PreviousPropsSame" select="count(/EntireModel/CellarModule[@id=$CellarModuleName]/class[@id=$classID]/props/*[@id=$propID]/preceding-sibling::*[@sig=$PotentialPhonyClass])"/>
					 <xsl:if test="$PreviousPropsSame=0">
						<!--Now check to see if any previous sibling classes in this cellar module have props with this signature. If so, (count<>0) then skip this.-->
						<xsl:variable name="PreviousClassPropsSame" select="count(/EntireModel/CellarModule[@id=$CellarModuleName]/class[@id=$classID]/preceding-sibling::*/props/*[@sig=$PotentialPhonyClass])"/>
						<xsl:if test="$PreviousClassPropsSame=0">
						   <xsl:for-each select="//class[@id=$PotentialPhonyClass]">
							  <xsl:if test="not(//CellarModule[@id=$CellarModuleName]/class[@id=$PotentialPhonyClass])">
								 <class>
									<xsl:attribute name="num">0</xsl:attribute>
									<!--The following mod attribute indicates the CellarModule where this "referenced" class comes from-->
									<xsl:attribute name="mod"><xsl:value-of select="../@id"/></xsl:attribute>
									<xsl:attribute name="id"><xsl:value-of select="@id"/></xsl:attribute>
									<xsl:attribute name="abbr">obj</xsl:attribute>
									<xsl:attribute name="abstract">false</xsl:attribute>
									<xsl:attribute name="base">CmObject</xsl:attribute>
									<xsl:attribute name="depth">0</xsl:attribute>
								 </class>
								 <xsl:text>&#xA;</xsl:text>
							  </xsl:if>
						   </xsl:for-each>
						</xsl:if>
					 </xsl:if>
				  </xsl:if>
			   </xsl:if>
			</xsl:for-each>
			<!--As above, we also need to check the base of any of the classes in this module. If any of the bases do not exist, then we need to create them, but only if they haven't been created before
				This means that we need to do the following:-->
			<!--0a. If the base class is CmObject (like most of them are), skip the rest because we've already inserted one of these-->
			<xsl:if test="@base!='CmObject'">
			   <!--0b. Find out the base and assign to variable PotentialPhonyClass2-->
			   <xsl:variable name="PotentialPhonyClass2" select="@base"/>
			   <!--1. If the base is a class that exists in this module then skip the rest-->
			   <xsl:if test="not(//CellarModule[@id=$CellarModuleName]/class[@id=$PotentialPhonyClass2])">
				  <!--2. Find out if any previous sibling classes had the same base. If so skip the rest.-->
				  <xsl:if test="count(/EntireModel/CellarModule[@id=$CellarModuleName]/class[@id=$classID]/preceding-sibling::*[@base=$PotentialPhonyClass2])=0">
					 <!--3a. Find out if this base class might be the same as one of the prop signatures for this class as above. If so skip the rest.-->
					 <xsl:if test="count(/EntireModel/CellarModule[@id=$CellarModuleName]/class[@id=$classID]/props/*[@sig=$PotentialPhonyClass2])=0">
						<!--3b. Find out if this base class might be the same as one of the other class prop signatures as above. If so skip the rest.-->
						<xsl:if test="count(/EntireModel/CellarModule[@id=$CellarModuleName]/class[@id=$classID]/preceding-sibling::*/props/*[@sig=$PotentialPhonyClass2])=0">
						   <!--4. If we've made it this far then we need to create the dummy class. -->
						   <class>
							  <xsl:attribute name="num">0</xsl:attribute>
							  <!--The following mod attribute indicates the CellarModule where this "referenced" class comes from-->
							  <xsl:attribute name="mod"><xsl:value-of select="//CellarModule[class[@id=$PotentialPhonyClass2]]/@id"/></xsl:attribute>
							  <xsl:attribute name="id"><xsl:value-of select="$PotentialPhonyClass2"/></xsl:attribute>
							  <xsl:attribute name="abbr">obj</xsl:attribute>
							  <xsl:attribute name="abstract">false</xsl:attribute>
							  <xsl:attribute name="base">CmObject</xsl:attribute>
							  <xsl:attribute name="depth">0</xsl:attribute>
						   </class>
						   <xsl:text>&#xA;</xsl:text>
						</xsl:if>
					 </xsl:if>
				  </xsl:if>
			   </xsl:if>
			</xsl:if>
		 </xsl:for-each>
		 <!--Here we begin the real classes-->
		 <xsl:for-each select="class">
			<!--xsl:sort select="@depth" order="ascending"/-->
			<xsl:if test="@id!='CmObject'">
			   <xsl:text>&#xA;</xsl:text>
			   <xsl:copy>
				  <!--copy node being visited-->
				  <xsl:copy-of select="@*"/>
				  <!--copy of all attributes-->
				  <xsl:text>&#10;</xsl:text>
				  <props>
					 <xsl:apply-templates/>
					 <xsl:text>&#xA;</xsl:text>
				  </props>
				  <!--process the children-->
			   </xsl:copy>
			   <xsl:text>&#xA;</xsl:text>
			</xsl:if>
		 </xsl:for-each>
	  </CellarModule>
   </xsl:template>
   <xsl:template match="class/props/*">
	  <xsl:text>&#xA;</xsl:text>
	  <xsl:copy>
		 <!--copy node being visited-->
		 <xsl:copy-of select="@*"/>
		 <!--copy of all attributes-->
		 <xsl:apply-templates/>
		 <!--process the children-->
	  </xsl:copy>
   </xsl:template>
</xsl:stylesheet>
