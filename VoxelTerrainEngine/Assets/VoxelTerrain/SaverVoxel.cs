/*
	PreviewLabs.PlayerPrefs
	April 1, 2014 version

	Public Domain
	
	To the extent possible under law, PreviewLabs has waived all copyright and related or neighboring rights to this document. This work is published from: Belgium.
	
	http://www.previewlabs.com
	
*/
using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.IO.Compression;

namespace VoxelEngine{
	public class SaverVoxel
	{
		private Hashtable playerPrefsHashtable;
		private bool hashTableChanged = false;
		private string serializedOutput = "";
		private string serializedInput = "";
		private const string PARAMETERS_SEPERATOR = ";";
		private const string KEY_VALUE_SEPERATOR = ":";
		private string[] seperators = new string[]{PARAMETERS_SEPERATOR,KEY_VALUE_SEPERATOR};
		public string fileName = "/PlayerPrefs.txt";
		private static readonly string secureFileName =  "/AdvancedPlayerPrefs.txt";
		//NOTE modify the iw3q part to an arbitrary string of length 4 for your project, as this is the encryption key
		private byte[] bytes = ASCIIEncoding.ASCII.GetBytes ("iw3q" + 0);
		private bool wasEncrypted = false;
		public SaverVoxel (string filename,string FolderName)
		{
			serializedInput = "";
			playerPrefsHashtable = new Hashtable();
			#if !UNITY_WEBPLAYER
			//load previous settings
			if (!Directory.Exists(FolderName))
				Directory.CreateDirectory(FolderName);

			if (File.Exists (secureFileName)) {
				//fileReader = new BinaryReader  (secureFileName);
				wasEncrypted = true;
				//serializedInput = Decrypt (fileReader.ReadToEnd ());
			} else if (File.Exists (FolderName + "/" +filename)) {

				byte [] values = File.ReadAllBytes(FolderName + "/" +filename );
				serializedInput = Encoding.ASCII.GetString(values);

			}
			#else
			
			if(UnityEngine.PlayerPrefs.HasKey("encryptedData")) {
				Debug.Log("error");
			}
			
			#endif
			
			if (!string.IsNullOrEmpty (serializedInput)) {
				//In the old PlayerPrefs, a WriteLine was used to write to the file.
				if (serializedInput.Length > 0 && serializedInput [serializedInput.Length - 1] == '\n') {
					serializedInput = serializedInput.Substring (0, serializedInput.Length - 1);
					
					if (serializedInput.Length > 0 && serializedInput [serializedInput.Length - 1] == '\r') {
						serializedInput = serializedInput.Substring (0, serializedInput.Length - 1);
					}
				}
				
				Deserialize ();
			}
			

			
			
		}
		public void reload(string filename, string FolderName)
		{
			serializedInput = "";
			playerPrefsHashtable.Clear();
#if !UNITY_WEBPLAYER
			//load previous settings
			if (!Directory.Exists(FolderName))
				Directory.CreateDirectory(FolderName);

			if (File.Exists(secureFileName))
			{
				//fileReader = new BinaryReader  (secureFileName);
				wasEncrypted = true;
				//serializedInput = Decrypt (fileReader.ReadToEnd ());
			}
			else if (File.Exists(FolderName + "/" + filename))
			{

				byte[] values = File.ReadAllBytes(FolderName + "/" + filename);
				serializedInput = Encoding.ASCII.GetString(values);

			}
#else
			
			if(UnityEngine.PlayerPrefs.HasKey("encryptedData")) {
				Debug.Log("error");
			}
			
#endif

			if (!string.IsNullOrEmpty(serializedInput))
			{
				//In the old PlayerPrefs, a WriteLine was used to write to the file.
				if (serializedInput.Length > 0 && serializedInput[serializedInput.Length - 1] == '\n')
				{
					serializedInput = serializedInput.Substring(0, serializedInput.Length - 1);

					if (serializedInput.Length > 0 && serializedInput[serializedInput.Length - 1] == '\r')
					{
						serializedInput = serializedInput.Substring(0, serializedInput.Length - 1);
					}
				}

				Deserialize();
			}




		}

		public bool HasKey (string key)
		{
			return playerPrefsHashtable.ContainsKey (key);
		}
		

		
		public void SetFloat (string key, float value)
		{
			if (!playerPrefsHashtable.ContainsKey (key)) {
				playerPrefsHashtable.Add (key, value);
			} else {
				playerPrefsHashtable [key] = value;
			}
			
			hashTableChanged = true;
		}
		/*
		public static void SetVector3 (string key, Vector3 value)
		{
			if (!playerPrefsHashtable.ContainsKey (key)) {
				playerPrefsHashtable.Add (key, value);
			} else {
				playerPrefsHashtable [key] = value;
			}
			
			hashTableChanged = true;
		}*/
		
		public void SetBool (string key, bool value)
		{
			if (!playerPrefsHashtable.ContainsKey (key)) {
				playerPrefsHashtable.Add (key, value);
			} else {
				playerPrefsHashtable [key] = value;
			}
			
			hashTableChanged = true;
		}


		
		public float GetFloat (string key)
		{			
			if (playerPrefsHashtable.ContainsKey (key)) {
				return (float)playerPrefsHashtable [key];
			}
			
			return 0.0f;
		}
		
		public float GetFloat (string key, float defaultValue)
		{
			if (playerPrefsHashtable.ContainsKey (key)) {
				return (float)playerPrefsHashtable [key];
			} else {
				playerPrefsHashtable.Add (key, defaultValue);
				hashTableChanged = true;
				return defaultValue;
			}
		}
		/*public static Vector3 GetVector3 (string key)
		{			
			if (playerPrefsHashtable.ContainsKey (key)) {
				return (Vector3)playerPrefsHashtable [key];
			}
			
			return new Vector3(0,0,0);
		}
		
		public static Vector3 GetVector3 (string key, Vector3 defaultValue)
		{
			if (playerPrefsHashtable.ContainsKey (key)) {
				return (Vector3)playerPrefsHashtable [key];
			} else {
				playerPrefsHashtable.Add (key, defaultValue);
				hashTableChanged = true;
				return defaultValue;
			}
		}*/
		
		public bool GetBool (string key)
		{			
			if (playerPrefsHashtable.ContainsKey (key)) {
				return (bool)playerPrefsHashtable [key];
			}
			
			return false;
		}
		
		public bool GetBool (string key, bool defaultValue)
		{
			if (playerPrefsHashtable.ContainsKey (key)) {
				return (bool)playerPrefsHashtable [key];
			} else {
				playerPrefsHashtable.Add (key, defaultValue);
				hashTableChanged = true;
				return defaultValue;
			}
		}
		
		public void DeleteKey (string key)
		{
			playerPrefsHashtable.Remove (key);
		}
		
		public void DeleteAll ()
		{
			playerPrefsHashtable.Clear ();
		}
		
		//This is important to check to avoid a weakness in your security when you are using encryption to avoid the users from editing your playerprefs.
		public bool WasReadPlayerPrefsFileEncrypted ()
		{
			return wasEncrypted;
		}
		

		
		public void Flush ()
		{	
			if (hashTableChanged) {
				Serialize ();
				
				string output = serializedOutput;


				File.Delete(fileName );

				byte[] values =  Encoding.ASCII.GetBytes(output);

				//fileWriter.Write (output);
				File.WriteAllBytes(fileName, values );
				//fileWriter.Close ();

				
				serializedOutput = "";
			}
		}

		private void Serialize ()
		{
			IDictionaryEnumerator myEnumerator = playerPrefsHashtable.GetEnumerator ();
			System.Text.StringBuilder sb = new System.Text.StringBuilder ();
			bool firstString = true;
			while (myEnumerator.MoveNext()) {
				//if(serializedOutput != "")
				if (!firstString) {
					sb.Append (" ");
					sb.Append (PARAMETERS_SEPERATOR);
					sb.Append (" ");
				}
				sb.Append (EscapeNonSeperators (myEnumerator.Key.ToString (), seperators));
				sb.Append (" ");
				sb.Append (KEY_VALUE_SEPERATOR);
				sb.Append (" ");
				sb.Append (EscapeNonSeperators (myEnumerator.Value.ToString (), seperators));
				sb.Append (" ");
				sb.Append (KEY_VALUE_SEPERATOR);
				sb.Append (" ");
				if(myEnumerator.Value.GetType().ToString() == "System.Single")
				sb.Append ("F");
				else if(myEnumerator.Value.GetType().ToString() == "System.Boolean")
					sb.Append ("B");
				firstString = false;
			}
			serializedOutput = sb.ToString ();
		}
		
		private void Deserialize ()
		{
			string[] parameters = serializedInput.Split (new string[] {" " + PARAMETERS_SEPERATOR + " "}, StringSplitOptions.RemoveEmptyEntries);
			
			foreach (string parameter in parameters) {
				string[] parameterContent = parameter.Split (new string[]{" " + KEY_VALUE_SEPERATOR + " "}, StringSplitOptions.None);
				
				playerPrefsHashtable.Add (DeEscapeNonSeperators (parameterContent [0], seperators), GetTypeValue (parameterContent [2], DeEscapeNonSeperators (parameterContent [1], seperators)));
				
				if (parameterContent.Length > 3) {
					Debug.LogWarning ("PlayerPrefs::Deserialize() parameterContent has " + parameterContent.Length + " elements");
				}
			}
		}
		
		public string EscapeNonSeperators(string inputToEscape, string[] seperators)
		{
			inputToEscape = inputToEscape.Replace("\\", "\\\\");
			
			for (int i = 0; i < seperators.Length; ++i)
			{
				inputToEscape = inputToEscape.Replace(seperators[i], "\\" + seperators[i]);
			}
			
			return inputToEscape;
		}
		
		public string DeEscapeNonSeperators(string inputToDeEscape, string[] seperators)
		{
			
			for (int i = 0; i < seperators.Length; ++i)
			{
				inputToDeEscape = inputToDeEscape.Replace("\\" + seperators[i], seperators[i]);
			}
			
			inputToDeEscape = inputToDeEscape.Replace("\\\\", "\\");
			
			return inputToDeEscape;
		}
		
		private string Encrypt (string originalString)
		{
			if (String.IsNullOrEmpty (originalString)) {
				return "";
			}
			
			DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider ();
			MemoryStream memoryStream = new MemoryStream ();
			CryptoStream cryptoStream = new CryptoStream (memoryStream, cryptoProvider.CreateEncryptor (bytes, bytes), CryptoStreamMode.Write);
			StreamWriter writer = new StreamWriter (cryptoStream);
			writer.Write (originalString);
			writer.Flush ();
			cryptoStream.FlushFinalBlock ();
			writer.Flush ();
			return Convert.ToBase64String (memoryStream.GetBuffer (), 0, (int)memoryStream.Length);
		}
		
		private string Decrypt (string cryptedString)
		{
			if (String.IsNullOrEmpty (cryptedString)) {
				return "";
			}
			DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider ();
			MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (cryptedString));
			CryptoStream cryptoStream = new CryptoStream (memoryStream, cryptoProvider.CreateDecryptor (bytes, bytes), CryptoStreamMode.Read);
			StreamReader reader = new StreamReader (cryptoStream);
			return reader.ReadToEnd ();
		}
		
		private object GetTypeValue (string typeName, string value)
		{

			if (typeName == "B") {
				return Convert.ToBoolean (value);
			}
			if (typeName == "F") { //float
				return Convert.ToSingle (value);
			}
			 
			/*if (typeName == "UnityEngine.Vector3") { //long
				return Convert.(value,value,value);
			}*/
			else {
				Debug.LogError ("Unsupported type: " + typeName);
			}	
			
			return null;
		}
	}	
}