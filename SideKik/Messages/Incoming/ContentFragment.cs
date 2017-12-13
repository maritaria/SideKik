using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml;

namespace SideKik.Messages.Incoming
{
	public sealed class ContentFragment : Fragment
	{
		public string AppId { get; }
		public string AppName { get; }
		public string Version { get; }
		public Dictionary<string, string> Strings { get; } = new Dictionary<string, string>();
		public Dictionary<string, string> Extras { get; } = new Dictionary<string, string>();
		public Dictionary<string, string> Images { get; } = new Dictionary<string, string>();
		public Dictionary<string, Uri> Uris { get; } = new Dictionary<string, Uri>();

		public ContentFragment(XmlNode node)
		{
			Contract.Requires(node != null);
			Contract.Requires(node.LocalName == "content");
			/*
				<content app-id="com.kik.ext.camera" id="[GUID]" v="2">
					<strings>
						<app-name>Camera</app-name>
						<file-size>49656</file-size>
						<allow-forward>true</allow-forward>
						<file-content-type>image/jpeg</file-content-type>
						<file-name>[GUID from content#id].jpg</file-name>
						<file-url>https://platform.kik.com/content/files/[GUID from content#id]?t=[KEY]</file-url>
					</strings>
					<extras/>
					<hashes>
						<sha1-original>B00C270632D11ECE461A487052394975A47EAA28</sha1-original>
						<sha1-scaled>E5CDB95540F7ADE7801CA152CC8C33227ABCEA92</sha1-scaled>
						<blockhash-scaled>00000001FFFEFF7F03A0FFFEFFF00000FFFFEFC700030010000F3FFFFC03001F</blockhash-scaled>
					</hashes>
					<images>
						<preview>LARGE_BLOB_OF_DATA</preview>
						<icon>LARGE_BLOB_OF_DATA</icon>
					</images>
					<uris/>
				</content>
			*/
			AppId = node.Attributes["app-id"].Value;
			Version = node.Attributes["v"].Value;
			var strings = node.ChildNodes[0];
			foreach (XmlNode child in strings)
			{
				Strings.Add(child.LocalName, child.InnerText);
			}
			var extras = node.ChildNodes[1];
			foreach (XmlNode child in extras)
			{
				Extras.Add(child.ChildNodes[0].InnerText, child.ChildNodes[1].InnerText);
			}
			var hashes = node.ChildNodes[2];
			var images = node.ChildNodes[3];
			foreach (XmlNode child in images)
			{
				Images.Add(child.LocalName, child.InnerText);
			}
			var uris = node.ChildNodes[4];
			foreach (XmlNode child in images)
			{
				Uris.Add(child.Attributes["platform"].Value, new Uri(child.InnerText));
			}
		}
	}
}