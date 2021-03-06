﻿using FluentFTP;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VxGuardian.Common;
using VxGuardian.Models;
using VxGuardian.EtcClass;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace VxGuardian.View
{
	/// <summary>
	/// Lógica de interacción para ConfiguracionFTP.xaml
	/// </summary>
	public partial class ConfiguracionFTP : Window
	{
		private Inicio ini;
		public FtpClient ftpClient;
		public Timer time;
		public Log gLog;
		public BackgroundWorker worker;
		private string TemporalStorage;
		private string newVersion;
		private BlackScreen bs;
		private double ProgressValue;
		bool Downloaded = false;
		bool initiated = false;
		private Etc tools;

		//GUSTAVO
		private List<ScreensGuardian> screenlist = new List<ScreensGuardian>();
		private List<string[]> pendientes = new List<string[]>();
		private List<string[]> versiones_anterior = new List<string[]>();
		private bool first ;


		public ConfiguracionFTP(Inicio _inicio)
		{
			ini = _inicio;
			gLog = new Log();
			time = new Timer();



			tools = new Etc();
			InitializeComponent();

			LoadInitialValuesINICIO();
			gLog.SaveLog("--- Modo FTP ---");

		}

		private void BtnIniciar_Click(object sender, RoutedEventArgs e)
		{
			Init();
		}

		public void Init()
		   {
			    if (CheckFieldsINICIAR())
			{
				this.Hide();
				ini.Minimize();
				SaveNewConfig();
				syncingOff();
				TemporalStorage = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxLine\\" + ini.config.CodePc;
				Etc.CreateDir(TemporalStorage);
				gLog.SaveLog("Directorio temporal creado Documentos/Voxline/numerComputador");
				CreateFTP();
				initiated = true;
				if (CheckConexionFTP())
				{
					//var ftp_directory = ftpClient.FileExists("lock.txt");
					//if(!(ftpClient.FileExists("lock.txt")))
					//{
						//gLog.SaveLog("80 - lock no existe , procede a sincronisar ");
						SyncAsync(ini.config.CodePc);
						
					//}
					//else
					//{
						//gLog.SaveLog("85 - lock encontrado ");
					//}
					if (!time.Enabled)
					{
						InitTime();
					}

				}

			}
			else
			{
				MessageBox.Show("Los campos obligatorios no se han llenado.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		public void SaveNewConfig()
		{
			ini.config.CodePc = TxtCodigo.Text.Trim();
			ini.config.CarpetaRaiz = TxtRaizSel.Text.Trim();
			ini.config.TiempoChequeo = TxtChequeo.Text.Trim();
			ini.config.Reproductor = TxtReproductor.Text.Trim();
			ini.config.ModeFtp[0].IpFtp = TxtIPFTP.Text.Trim();
			ini.config.ModeFtp[0].Puerto = TxtPuerto.Text.Trim();
			ini.config.ModeFtp[0].Usuario = TxtUsuario.Text.Trim();
			ini.config.ModeFtp[0].Contrasena = TxtContrasena.Password.Trim();
			ini.config.SelectedMode = "FTP";
			ini.db.Save(ini.config);
		}

		//public void ConnectionDB_OLD()
		//{
		//	try
		//	{
		//		//db = new VoxContext();
		//		db = new VoxContext();
		//		root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(ini.db.fileJsonDir));
		//		config = root.Config[0];
		//		new System.Threading.Thread(() =>
		//		{
		//			globalLog.Logger("\r\n \r\n ------ Inicio MODO FTP", config.CodePc.ToString().Replace(" ", ""));
		//		}).Start();
		//	}
		//	catch (Exception e)
		//	{
		//		new System.Threading.Thread(() =>
		//		{
		//			globalLog.Logger("\r\n \r\n No se pudo conectar con la base de datos local", "DB");
		//		}).Start();
		//		//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
		//		Environment.Exit(1);
		//		//throw;
		//	}
		//}

		public void LoadInitialValuesINICIO()
		{
			TxtCodigo.Text = ini.config.CodePc;
			TxtRaizSel.Text = ini.config.CarpetaRaiz;
			TxtChequeo.Text = ini.config.TiempoChequeo;
			TxtReproductor.Text = ini.config.Reproductor;
			TxtIPFTP.Text = ini.config.ModeFtp[0].IpFtp;
			TxtPuerto.Text = ini.config.ModeFtp[0].Puerto;
			TxtUsuario.Text = ini.config.ModeFtp[0].Usuario;
			TxtContrasena.Password = ini.config.ModeFtp[0].Contrasena;
		}



		public Boolean CheckFieldsINICIAR()
		{
			if (
					!Etc.CheckFieldsTBOX(TxtCodigo) ||
					!Etc.CheckFieldsTBOX(TxtRaizSel) ||
					!Etc.CheckFieldsTBOX(TxtChequeo) ||
					!Etc.CheckFieldsTBOX(TxtIPFTP) ||
					!Etc.CheckFieldsTBOX(TxtPuerto) ||
					!Etc.CheckFieldsTBOX(TxtUsuario) ||
					!Etc.CheckFieldsPBOX(TxtContrasena)
				)
			{
				return false;
			}
			return true;
		}

		public void CreateFTP()
		{
			//Datos
			int port = Int32.Parse(ini.config.ModeFtp[0].Puerto);
			string ip = ini.config.ModeFtp[0].IpFtp;
			string user = ini.config.ModeFtp[0].Usuario;
			string password = ini.config.ModeFtp[0].Contrasena;

			// create an FTP

			try
			{
				ftpClient = new FtpClient(ip, user, password);
				ftpClient.Port = port;
				ftpClient.EncryptionMode = FtpEncryptionMode.Explicit;
				ftpClient.SslProtocols = SslProtocols.Default;
				ftpClient.ValidateCertificate += new FtpSslValidation(delegate (FtpClient c, FtpSslValidationEventArgs e)
				{
					e.Accept = true;
				});
				// cambio gonzalo
				ftpClient.DataConnectionType = FtpDataConnectionType.PASV;
				ftpClient.ReadTimeout = 5000;
				ftpClient.RetryAttempts = 3;
			}
			catch (Exception ex)
			{
				gLog.SaveLog(ex.Message);
			}

		}

		public void CloseConnection()
		{
			if (ftpClient.IsConnected)
			{
				ftpClient.Disconnect();
			}

		}

		public Boolean CheckConexionFTP()
		{
			if (!ftpClient.IsConnected)
			{
				try
				{
					ftpClient.Connect();
					//gLog.SaveLog("Conectado al servidor FTP");
				}
				catch (Exception ex)
				{
					gLog.SaveLog("No se logro conexion con el servidor ftp -- " + ex.Message);
					//Console.WriteLine(ex.ToString());
					return false;
				}
			}
			return true;
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//if (ftpClient.IsConnected){
			//	DisconnectFTP();
			//}
			//Environment.Exit(0);
		}

		private void BtnConfig_Click(object sender, RoutedEventArgs e)
		{
			ini.Show();
			this.Hide();
		}

		private void DisconnectFTP()
		{
			ftpClient.Disconnect();
		}


		public void createRemoteFoder(string dir, FtpClient _ftpclient)
		{
			if (!_ftpclient.DirectoryExists(dir))
			{
				_ftpclient.CreateDirectory(dir);
			}
		}

		public bool checkRemoteFolder(string dir, FtpClient _ftpclient)
		{
			if (!_ftpclient.DirectoryExists(dir))
			{
				return false;
			}
			return true;
		}


		private void InitTime()
		{

			double interval = Double.Parse(ini.config.TiempoChequeo);
			time = new Timer(Etc.minsToMS(interval));
			time.Elapsed += Timer_Elapsed;
			time.AutoReset = true;
			time.Start();
			gLog.SaveLog("278 - TIME START");

		}

		public void StopTime()
		{
			time.Stop();
			gLog.SaveLog("285 - TIME STOP");
		}
		
		//Gustavo modificacion antes : != 0 Desactivado , ahora == 0  activado
		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{ 
			if(ini.config.Syncing == 0)
			{
				if (time.Enabled)
				{
					StopTime();
				}
				SyncAsync(ini.config.CodePc);
				if (!time.Enabled)
				{
					InitTime();
				}
			}
		
		}



		//Sincronizacion asyncrona
		public void SyncAsync(string _remotePath)
		{
			string Path = _remotePath;
			int Qty = 0;

			//Chequea que exista la carpeta en el ftp 
			if (checkRemoteFolder(Path, ftpClient))
			{
				
					try
					{
						//GUSTAVO
						//archivos en el ftp
						//var archivos_ftp = ftpClient.GetListing();

						gLog.SaveLog(" 313 - DOWNLOAD FILE ASYNC");
						gLog.SaveLog(" 313 - REMOTE PAD = "+ Path);

						//Descarga Contenido.
						DownloadFilesAsync(ftpClient, Path);


						CloseConnection();
						gLog.SaveLog("Se sincronizo archivos");
					}
					catch (Exception ex)
					{
						gLog.SaveLog("320 No se logro sincronizar --" + ex.Message);
						//throw;
					}

					//si hubo descargas
					if (Downloaded)
					{
						try
						{

							//CreateBS();
							if (Etc.KillApp(ini.config.Reproductor))
							{
								gLog.SaveLog("333 - KILL APP TRUE");
							}
							else
							{
								gLog.SaveLog("333 - KILL APP FALSE");
							}
								 //Cierra el reproductor 
							gLog.SaveLog("333 - Cierra el reproductor");
						//gLog.SaveLog("334 - Pausa por 3000");
						//System.Threading.Thread.Sleep(3000);
						//gLog.SaveLog("336 - Fin pausa");

						//CopyTemporalToDirAsync(TemporalStorage, ini.config.CarpetaRaiz); //Copia el directorio temporal a la carpeta raiz



						//Copiar videos de la definitva a la temporal
						/* --------------------------------- */
						///
						//Copia la carpeta temporal a la definitva
						//GUSTAVO

						gLog.SaveLog("368 - Antes de copiar archivos pendientes");
						foreach (var archivo in  pendientes)
							{							
								if(archivo[2] != "lock.txt")
								{
								if (!(Directory.Exists(archivo[1])))
								{
									Directory.CreateDirectory(archivo[1]);
								}
									Etc.CopyFile(archivo[0], archivo[1] + "\\" + archivo[2]);
									gLog.SaveLog("373 = archivo copiado : " + archivo[2]);
									var directorio = ftpClient.GetListing(Path);
									var asd = ini.config.CodePc;
									//Etc.CopyFile(rutaarchivoftp , ScreenTemporal + "\\" + video);
								}
							}
						gLog.SaveLog("383 - Copio archivos pendientes");
						pendientes.Clear();
						gLog.SaveLog("385 - limpio lista de pendientes");


						gLog.SaveLog("341 - Entra al metodo copiar  directorio temporal " + TemporalStorage + " a la carpeta raiz " + ini.config.CarpetaRaiz);
						CopyTemporalToDirAsync2(TemporalStorage, ini.config.CarpetaRaiz); //Copia el directorio temporal a la carpeta raiz 
						

							//OpenApp(ini.config.Reproductor);
							//CloseBS();
							Downloaded = false;
							syncingOff();
						}
						catch (Exception ex)
						{
							gLog.SaveLog(ex.Message);
							//Debug.WriteLine(ex.Message);
							//throw;
						}

						//gustavo copya el json a la carpeta final antes de borrar la temporal
						try
						{
							CopyJsonPlayList(TemporalStorage, ini.config.CarpetaRaiz);//Copia el json PLayList del directorio temporal a la carpeta raiz
							gLog.SaveLog("358 - Copia el json de la carpeta temporal a la definitiva");
						}
						catch (Exception ex)
						{

							gLog.SaveLog("363 - ERROR COPY PlayList.json " + ex.Message);
						}
						//cambio gonzalo
						try
						{
							//gustavo
							if(Directory.Exists(TemporalStorage))
							{
								gLog.SaveLog("374 - Borra archivos de la carpeta temporal.");
								Etc.DeleteFiles(TemporalStorage);
							}
						
						}
						catch (Exception ex)
						{

							gLog.SaveLog("ERROR CLEAR TEMPORAL " + ex.Message);
						}
					}

					gLog.SaveLog("394 - Open App ");
					Etc.OpenApp(ini.config.Reproductor);
				
			} // fin check remote folder

		}

		private void CreateBS()
		{
			System.Threading.Thread _thread = new System.Threading.Thread(() =>
			{
				try
				{
					bs = new BlackScreen();
					bs.Show();
				}
				catch
				{ }
			});
			_thread.Start();

		}

		private void CloseBS()
		{
			System.Threading.Thread _thread = new System.Threading.Thread(() =>
			{
				try
				{
					bs = new BlackScreen();
					bs.Close();
				}
				catch
				{ }
			});
			_thread.Start();
		}

		void syncingOn()
		{
			ini.config.Syncing = 1;
			ini.db.Save(ini.config);
		}

		void syncingOff()
		{
			ini.config.Syncing = 0;
			ini.db.Save(ini.config);
		}

		//Gustavo
		private void CopyTemporalToDirAsync2(string _temporalFolder, string _destinyFolder)
		{
			
			try
			{
				if (Directory.Exists(_temporalFolder))
				{
					//comprueba que exista el json 
					if (File.Exists(_temporalFolder + "\\" + "PlayList.json"))
					{
						
						//////////////Funcio leer json 
						///Lee un archivo json 
						gLog.SaveLog("464 Lee el json ");
						var json = Etc.ReadJson(_temporalFolder);
						var computers = json["computers"];

						//recorre la lista de computadores en el JSON
						//gLog.SaveLog("466 - LEE EL JSON  - Computadores ");
						foreach (var computer in computers)
						{
							string pccode = computer["code"].ToString();
							//No se crea una carpeta por computador SOLO PANTALLAS
							if (pccode.Equals(ini.config.CodePc.ToString())) 
							{
								//Recorre la lista de pantallas por computador en el JSON
								gLog.SaveLog("473 - Ciclo Pantallas por computador ");
								
								foreach (var screen in computer["screens"])
								{
									string code = screen["code"].ToString();
									var jsonversion = screen["version"].ToString();
									string actualversion = versiones_anterior.Find(x => x[0].ToString().Equals(code))[1].ToString();
									/*if (first)
									{
										actualversion = "0";
									}*/

									if((!(actualversion.Equals(jsonversion))) )
									{ 
										versiones_anterior.Find(x => x[0].ToString().Equals(code))[1] = jsonversion;
										
										//nombre de la carpetas
										string screen_folder_name = "p" + screen["code"];

										//Borrar directiorio definitvo existente actual
										if (Directory.Exists(_destinyFolder + "\\" + screen_folder_name))
										{
											Etc.DeleteFiles(_destinyFolder + "\\" + screen_folder_name);
											gLog.SaveLog("499 - Limpiop el directorio de destino : " + _destinyFolder);
											Etc.CreateLock(_destinyFolder + "\\" + screen_folder_name);
											gLog.SaveLog("501 - Crea lock en :" + _destinyFolder + "\\" + screen_folder_name);
										}else
										{
											//crea el directorio definitivo
											Directory.CreateDirectory(_destinyFolder + "\\" + screen_folder_name);
											gLog.SaveLog("545 - Crea el directorio definitivo :  " + screen_folder_name);
											Etc.CreateLock(_destinyFolder + "\\" + screen_folder_name);
											gLog.SaveLog("547 - Crea lock en :" + _destinyFolder + "\\" + screen_folder_name);
										}


										//extrae la version de la pantalla
										var screen_version = screen["version"];

										//url del archivo de version temporal
										string version_temp_path = _temporalFolder + "\\" + screen_folder_name + "\\" + "v" + screen_version + ".txt";

										//url del archivo de version definitivo
										string version_destiny_path = _destinyFolder + "\\" + screen_folder_name + "\\" + "v" + screen_version + ".txt";

										//comprueba si existe el archivo de version temporal
										if (File.Exists(version_temp_path))
										{
											//copia el archivo de version temporal a la carpeta definitiva
											File.Copy(version_temp_path, version_destiny_path);
											gLog.SaveLog("498 -Copia la version de la temporal a la definitiva");

											//recorre los contenido de la playlist en el json 
											gLog.SaveLog("501 - Ciclo Playlist , copia el cotenido de la temporal a la definitiva siguiendo el playlist.json ");
											foreach (var content in screen["playlist"])
											{
												//nombre del contenido
												string content_name = content["name"].ToString();
												//id del contenido
												string content_original_id = content["originalID"].ToString();
												//posicion del contenido
												string content_position = content["defOrder"].ToString();

												//url del contenido temporal id-nombre-nombre.mp4
												string content_temp_path = _temporalFolder + "\\" + screen_folder_name + "\\" + content_original_id + "-" + content_name + ".mp4";
												//url del contenido definitivo orden-id-nombre.mp4
												string content_destiny_path = _destinyFolder + "\\" + screen_folder_name + "\\" + content_position + "-" + content_original_id + "-" + content_name + ".mp4";

												//Comprueba si existe el contenido temporal
												if (File.Exists(content_temp_path))
												{
													//Copia el contenido temporal a la carpeta definitiva
													File.Copy(content_temp_path, content_destiny_path);
													gLog.SaveLog("579 - Copia el contenido temporal a la carpeta definitiva : "+ content_name);
												}
											}//end foreach playlist

											//GUSTAVO 
											//Cierra el text reader
											//file.Close();
											//gLog.SaveLog("528 - Cierra el textreader de playlist.json");
										}
										else
										{
											gLog.SaveLog("531 - No se encontro el archvio (Version).txt : "+ version_temp_path);
										}

										Etc.DeleteLock(_destinyFolder + "\\" + screen_folder_name);
										gLog.SaveLog("595 - Borra el lock de la carpeta definitva : " + _destinyFolder + "\\" + screen_folder_name);
										Etc.DeleteLock(_temporalFolder + "\\" + screen_folder_name);
										gLog.SaveLog("597 - Borra el lock de la carpeta temporal : " + _temporalFolder + "\\" + screen_folder_name);
									}

								}//end foreach screens
								

							}
						}//end foreach computers

						


					}else
					{
						gLog.SaveLog("543 - PlayList.Json  no existe en la carpeta temporal");
					}

				}
				else
				{
					Console.WriteLine("549 - Source path does not exist!");
				}
			}
			catch (Exception ex)
			{
				gLog.SaveLog("554 - No se logro copiar archivos -- " + ex.Message);
				//throw;
			}
		}

		private void CopyTemporalToDirAsync(string _temporalFolder, string _destinyFolder)
		{
			try
			{
				if (Directory.Exists(_temporalFolder))
				{
					Copy(_temporalFolder, _destinyFolder, ini, ftpClient);
				}
				else
				{
					Console.WriteLine("569 - Source path does not exist!");
				}
			}
			catch (Exception ex)
			{
				gLog.SaveLog("574 - No se logro copiar archivos -- " + ex.Message);
				//throw;
			}
		}

		public static void Copy(string sourceDirectory, string targetDirectory, Inicio _ini, FtpClient ftpClient)
		{
			DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
			DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

			CopyAll(diSource, diTarget, _ini, ftpClient, null);
		}

		//GUSTAVO metodo para copiar el json de la carpeta temporal a la carpeta final
		public static void CopyJsonPlayList(string source, string target )
		{
			//Copiar PlayList
			//var exist_json = File.Exists(target.FullName + "\\PlayList.json");


			if (File.Exists(target + "\\PlayList.json"))
			{
				//SI el archivo existe
				//Borra el archivo 
				File.Delete(target + "\\PlayList.json");
				//Copia el archivo
				File.Copy(source + "\\PlayList.json", target + "\\PlayList.json");
			}
			else
			{
				//si no existe copia el
				File.Copy(source + "\\PlayList.json", target + "\\PlayList.json");
			}
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target, Inicio _ini, FtpClient ftp, ScreensGuardian _screen = null)
		{

			ScreensGuardian screenAUX = _screen;

			var _screens = _ini.config.Screens.ToArray();

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				foreach (ScreensGuardian screen in _screens)
				{
					if ("p" + screen.Code == diSourceSubDir.Name)
					{
						DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
						CopyAll(diSourceSubDir, nextTargetSubDir, _ini, ftp, screen);
						Log TgLog = new Log();
						TgLog.SaveLog("625 - DIRECTORIO COPIADO " + diSourceSubDir.Name);
					}
				}
			}

			{
				Directory.CreateDirectory(screenAUX.LocalPath);
				//cambio gonzalo
				try
				{
					Etc.DeleteFiles(screenAUX.LocalPath);
					System.Threading.Thread.Sleep(2000);
				}
				catch (Exception ex)
				{
					Log logaux = new Log();
					logaux.SaveLog("642 - ERROR CLEAR " + ex.Message);
				}

				// Copy each file into the new directory.
				foreach (FileInfo fi in source.GetFiles())
				{
					//Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
					
					fi.CopyTo(Path.Combine(screenAUX.LocalPath, fi.Name), true);
					Log TgLog = new Log();
					TgLog.SaveLog("652 - ARCHIVO COPIADO - " + Path.Combine(screenAUX.LocalPath, fi.Name));
				}

				int idx = _ini.config.Screens.FindIndex(s => s.Code == screenAUX.Code);
				//guardar version nueva
				screenAUX.VersionActual = screenAUX.VersionRemota.ToString();
				_ini.config.Screens[idx] = screenAUX;
				_ini.db.Save(_ini.config);
			}
		}

		private void OpenApp(string _dir)
		{
			Process proc = new Process();
			proc.StartInfo.FileName = @_dir;
			proc.Start();
			System.Threading.Thread.Sleep(5000);
			//Process.Start(@_dir);
		}

		//DESCARGA el contenido del ftp a la carpeta temporal
		private void DownloadFilesAsync(FtpClient _ftpclient, string _remotePath)
		{

			string Path = _remotePath;
			gLog.SaveLog(" 676 download asinc PATH :  " + Path);
			string TemporalLocalFolder = TemporalStorage;
			Downloaded = false;

			//GUSTAVO 			
			//string root_temp_adress = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxLine\\PlayList.json";
			//Descargae el json a la carpeta raiz
			//JSON var directorio = _ftpclient.GetListing("PlayList.json");
			//var directorio = _ftpclient.GetListing(Path + "\\p859");
			//var pantalla = directorio[0];


			//GUSTAVO
			var existlist = false;
			//if (screenlist.Count() == 0)
			//{
				//Descargar el json 
				////////////////////////////////////////////////////////////////////////////////Si existe no descargar?
				gLog.SaveLog("740 - PLAYLIST. Antes de descargar el JSON ");
				ftpClient.DownloadFile(TemporalLocalFolder + "\\PLayList.json", "PlayList.json", FtpLocalExists.Overwrite, FtpVerify.Retry);
				gLog.SaveLog("742 - PLAYLIST. JSON descargado");
				existlist = false;

			//}
			//else
			//{
				//gLog.SaveLog("746 no descarga el json ");
				//existlist = true;
			//}
			

			//Etc.CreateDir(TemporalLocalFolder);

			int auxI = 0;

			//ini.config.Screens.Clear();
			//getScreens
			gLog.SaveLog("698 - Ciclo for para guardar las pantallas en memoria");
			///Gustavo 
			//var directorioftp = _ftpclient.GetListing(Path+ "p759");
			var directorioftp2 = _ftpclient.GetListing(Path);
			//
			foreach (FtpListItem item in _ftpclient.GetListing(Path).OrderByDescending(item => item.Name))
			{
				var pantallaname = item.FullName;
				var pname = pantallaname.Split('_');
				int largosplit = pname.Count();
				if(!( pname[largosplit - 1].ToString().ToLower().Equals("tmp")))
				{
					//if the folder is the player folder, enter and download
					if (item.Type == FtpFileSystemObjectType.Directory && item.Name.Substring(0, 1) == "p")
					{
						string code = item.Name.Substring(1, item.Name.Length - 1);
						if (ini.config.Screens.Exists(x => x.Code == code))
						{
							first = false;
							int idx = ini.config.Screens.FindIndex(x => x.Code == code);
							ini.config.Screens[idx].Nombre = item.Name;
							ini.config.Screens[idx].Path = item.FullName;
							ini.config.Screens[idx].Code = code;
							ini.config.Screens[idx].LocalPath = ini.config.CarpetaRaiz + "\\p" + code;
						}
						else
						{
							first = true;
							ScreensGuardian _screen = new ScreensGuardian();
							_screen.Nombre = item.Name;
							_screen.Path = item.FullName;
							_screen.Code = code;
							_screen.LocalPath = ini.config.CarpetaRaiz + "\\p" + code;
							_screen.VersionActual = "0";
							ini.config.Screens.Add(_screen);

						}

						ini.db.Save(ini.config);
						auxI++;
					}
				}
				else
				{
					gLog.SaveLog("840 - Se salto la pantalla temporal");
				}
			}

				

			var _screens = ini.config.Screens.ToArray();
			//GUSTAVO
			if(screenlist.Count()>0)
			{
				gLog.SaveLog("797 - Descarga pantallas resagadas");
				_screens = screenlist.ToArray();
			}

			gLog.SaveLog("736 - Ciclo for para recorrer las pantallas guardadas en memoria");
			int aux = 0;


			//versiones_anterior.Clear();
			
			foreach (ScreensGuardian screen in _screens)
			{
				
				
				if ( (!(Etc.CheckRemoteLock(_ftpclient , screen.Path))) && (!(Etc.CheckLock(screen.LocalPath))) ) 
				{
					
					gLog.SaveLog("821 - NO existe el lock procede a descargar pantalla : " + screen.Path);
					//ACA crea un lock remoto por pantalla y al terminar lo borra

					//GUSTAVO
					var locktxt =  Etc.CreateRemoteLock(_ftpclient, screen.Path , screen.LocalPath);
					Etc.CreateLock(screen.LocalPath);
					gLog.SaveLog("826 - Creo lock en FTP , Local definitiva pantalla : "+ screen.Path)	;
					//var subiolock = ftpClient.GetListing(screen.Path);
					//////////////////////////////////

					
				
					gLog.SaveLog(" 740 GETREmote version antes");
					int _versionRemota = GetRemoteVersion(_ftpclient, screen.Path);
					gLog.SaveLog(" 742 GETREmote version despues " );


					screen.VersionRemota = _versionRemota;
					ini.config.Screens[aux] = screen;
					ini.db.Save(ini.config);

					//----------------------------------------------------------
					//
					
						//crea un array con el codigo de la pantalla y la version
						
						//en la primera vuelta agrega el codigo pantalla y la  version anterior al array
						if (first)
						{
							string[] versionanterior = new string[2];
							versionanterior[0] = screen.Code.ToString();
							versionanterior[1] = screen.VersionActual.ToString();
							versiones_anterior.Add(versionanterior);						
						}
						else						
						{
							//busca la pantalla en la lista y le asigna la version actual 
							var pantalla = versiones_anterior.Find(p => Int32.Parse(p[0]) == Int32.Parse(screen.Code));
							if(pantalla != null)
							{
								versiones_anterior.Find(p => Int32.Parse(p[0]) == Int32.Parse(screen.Code))[1] = screen.VersionActual.ToString();
							}else
							{
								string[] versionanterior = new string[2];
								versionanterior[0] = screen.Code.ToString();
								versionanterior[1] = screen.VersionActual.ToString();
								versiones_anterior.Add(versionanterior);
							}
							
						}
						



					//Si No existe el directorio  o la version remota es mayor a la actual
					gLog.SaveLog(" 753 ANTES del if de comprueva version ");
					if (!Etc.CheckDir(screen.LocalPath) || screen.VersionRemota > Int32.Parse(screen.VersionActual))
					{
						gLog.SaveLog(" 756 En el if de comprueva version ");
						
						
						
						//le cambia la version actual a la pantalla.
						screen.VersionActual = screen.VersionRemota.ToString();

						string ScreenTemporal = TemporalLocalFolder + '\\' + screen.Nombre;
						try
						{
							
							if (!Directory.Exists(ScreenTemporal))
							{
								Etc.CreateDir(ScreenTemporal);
								gLog.SaveLog("767 - CREA DIRECTORIO TEMPORAL : " + screen.Nombre);
								Etc.CreateLock(ScreenTemporal);
							}
							else
							{
									Etc.ClearDir(ScreenTemporal);
									gLog.SaveLog("772 - CLEARDIR (Limpia EL DIRECTORIO) : " + screen.Nombre);
									Etc.CreateLock(ScreenTemporal);
							}
							
						}
						 catch (Exception ex)
						{

							gLog.SaveLog("778 - ERROR Create or Clear Directory " + ex.Message);
						}




						//Descarga los dentro de la carpeta correspondiendte a la pantalla en el ftp 
						gLog.SaveLog("785 - Ciclo para descargar los archivos a la temporal");
						
						foreach (FtpListItem item in _ftpclient.GetListing(screen.Path).OrderByDescending(item => item.Name))
						{ 
							 if (item.Type == FtpFileSystemObjectType.File)
							  {
								  string downloadFileName = ScreenTemporal + "\\" + item.Name;
								FileInfo f = new FileInfo(downloadFileName);
								try
								 {
									//GUSTAVO
									string rutaarchivoftp = screen.LocalPath + "\\" + item.Name; //Ruta del archivo en la carpeta definitiva		
									var directorio = Directory.Exists(rutaarchivoftp);
									string new_video_name = "";
									string extencion = "";
									var encontro = false;
									var orderdef = "";


									if (Directory.Exists(screen.LocalPath))
									{
										//GUSTAVO
										//Extraer lae extencion

										var item_separated = (item.Name).Split('.');
										if (item_separated.Length > 0)
										{
											extencion = item_separated[item_separated.Length - 1];

										}

										//Si lae extencion es txt , corresponde a la version y lo copia sin necesidad de modificar el nombre.
										if(extencion != "txt")
										{
											//MODIFICAR LA RUTA PARA SABER QUE ARCHIVO COPIAR									
											var archivos = Directory.GetFiles(screen.LocalPath);
											foreach (var archivo in archivos)
											{
												new_video_name = archivo.Remove(0, (screen.LocalPath + "\\").Length);
												var separated_video_name = new_video_name.Split('-');
											
												new_video_name = "";
												for (int i = 0; i < separated_video_name.Length;  i++)
												{
													//omite la primera posicion
													if(i==0)
													{
														orderdef = separated_video_name[i];
													}
													//agrega - para volver a unir las partes
													if (i > 0 && i < separated_video_name.Length - 1)
													{
														new_video_name = new_video_name + separated_video_name[i] + "-";
													}
													//evita poner - en la ultima posicion del arreglo
													if (i == separated_video_name.Length - 1)
													{
														new_video_name = new_video_name + separated_video_name[i];
													}
												}

												//Comparacion de nombres 
												string ruta_video_name = rutaarchivoftp.Remove(0, (screen.LocalPath + "\\").Length);
												if(ruta_video_name == new_video_name)
												{
													encontro = true;
													rutaarchivoftp = screen.LocalPath + "\\" + orderdef + "-" + ruta_video_name;
													break;
												}
											



											}

											//----------------------------
										}



										if (File.Exists(rutaarchivoftp))
										{
											//Crear un array con los videos que tienen que ser copiados

											
											string[] pendiente = new string[3];
											pendiente[0] = rutaarchivoftp;
											pendiente[1] = ScreenTemporal;
											pendiente[2] = item.Name;

											pendientes.Add(pendiente);
											gLog.SaveLog("954 - Agrego a la lista para copiar luego " + item.Name);
											//Etc.CopyFile(rutaarchivoftp , ScreenTemporal + "\\" + item.Name);
											Downloaded = true;
										}
										else
										{
											if (ftpClient.DownloadFile(downloadFileName, item.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry))
											{
												Downloaded = true;
												gLog.SaveLog("875 - DESCARGADO " + item.Name);
											}
											else
											{
												gLog.SaveLog("879 - ERROR EN " + item.Name);
											}
										
										}

									}else
									{
										if (ftpClient.DownloadFile(downloadFileName, item.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry))
										{
											Downloaded = true;
											gLog.SaveLog("889 - DESCARGADO " + item.Name);
										}
										else
										{
											gLog.SaveLog("893 - ERROR EN " + item.Name);
										}
									}
								
								
									/* ANTES
										if (ftpClient.DownloadFile(downloadFileName, item.FullName, FtpLocalExists.Overwrite, FtpVerify.Retry))
									 {
										 Downloaded = true;
										 gLog.SaveLog("DESCARGADO " + item.Name);
									 }
									else
									{
										gLog.SaveLog("ERROR EN " + item.Name);
									}*/

								}
								catch (Exception ex)
								{
									gLog.SaveLog(ex.Message);
									//throw;
								}
							}
							//Gustavo
							

						}//Fin del foreach descarga archivos a la temporal
						var checklock = Etc.DeleteRemoteLock(_ftpclient, screen.Path);						
						gLog.SaveLog("1051 - Borro lock en  FTP  : " + screen.Path);
						

					}
					else
					{
						gLog.SaveLog(" 921 no hay version nueva");
						var checklock = Etc.DeleteRemoteLock(_ftpclient, screen.Path);
						gLog.SaveLog("1051 - Borro lock en  FTP  : " + screen.Path);
						Etc.DeleteLock(screen.LocalPath);						
						gLog.SaveLog("1052 - Borro lock en carpeta definitiva    : " + screen.LocalPath);

						//busca la pantalla en la lista y le asigna la version actual 
						//versiones_anterior.Find(p => Int32.Parse(p[0]) == Int32.Parse(screen.Code))[1] = screen.VersionActual.ToString();

						//Etc.DeleteFiles(TemporalLocalFolder);
						//gLog.SaveLog("1052 - Borro archivos temporales  : " + screen.LocalPath);
					}
					aux++;
					if(screenlist.Count > 0 )
					{
						if (screenlist.Contains(screen))
						{
							screenlist.Remove(screen);
						}
					}
						
					
				}
				else
				{
					gLog.SaveLog(" 976 - Existe el lock por pantalla : " + screen.Path);
					gLog.SaveLog(" 977 - Guarda la pantalla en un arreglo y pasa a la siguiente : " + screen.Path);

					if(!(screenlist.Contains(screen)))
					{
						screenlist.Add(screen);
					}else
					{
						gLog.SaveLog(" 1010 -La pantalla ya se encuentra agregada a lista de pantallas faltantes : " + screen.Path);
					}
					

					//Guardar pantallas en un arreglo.

				}
				
				gLog.SaveLog("918 -  Fin del cliclo que comprueba version");
				
			} //Fin foreach descarga contenido de pantallas



			//Pantallas resagadas
			/////////////////////////////////////
			/////////////////////////////
			//////////////////////////
			////////////////////////////
			///
			/*foreach(var screen in screenlist)
			{

			}*/




			CloseConnection();
			gLog.SaveLog("928 - Cierra Coneccion con BD ya termino de descargar a las temporales");
				
		}

		private void SelRaiz_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
			dialog.RootFolder = Environment.SpecialFolder.Desktop;
			if (dialog.ShowDialog(this).GetValueOrDefault())
			{
				TxtRaizSel.Text = dialog.SelectedPath;
			}
		}

		private void SelReproductor_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog
			{
				AddExtension = true,
				Filter = "VxPlayer.exe|*.exe|VxPlayer.ink|*.ink",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
			};

			if (dialog.ShowDialog(this).GetValueOrDefault())
			{
				TxtReproductor.Text = dialog.FileName;
			}
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			Etc.SelectAddress(sender, e);
		}

		private void SelectPassword(object sender, RoutedEventArgs e)
		{
			Etc.SelectAddress(sender, e);
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			Etc.SelectivelyIgnoreMouseButton(sender, e);
		}




		public void DeleteAllVersions(string _folderScreen)
		{
			var filesOnScreenFolder = Directory.GetFiles(_folderScreen).OrderByDescending(f => f);

			foreach (var file in filesOnScreenFolder)
			{
				string fileNew = file.Split('\\').Last().ToString();
				if (fileNew.Substring(0, 1) == "v")
				{
					File.Delete(file);
				}
			}
		}

		public int GetVersion(string _folderScreen)
		{
			var filesOnScreenFolder = Directory.GetFiles(_folderScreen).OrderByDescending(f => f);

			try
			{
				foreach (var file in filesOnScreenFolder)
				{
					string fileNew = file.Split('\\').Last().ToString();
					if (fileNew.Substring(0, 1) == "v")
					{
						return Int32.Parse(fileNew.Substring(1, fileNew.Length - 1));
					}
				}
			}
			catch (Exception)
			{
				return 0;
				//throw;
			}

			return 0;


		}

		public static int GetRemoteVersion(FtpClient _ftpclient, string _path)
		{
			try
			{
				if (_ftpclient.DirectoryExists(_path))
				{
					foreach (FtpListItem item in _ftpclient.GetListing(_path).OrderByDescending(item => item.Name))
					{
						if (item.Type == FtpFileSystemObjectType.File && item.Name.Substring(0, 1) == "v")
						{
							string nombreFinal = item.Name.Split('.').First();
							int lengthFinal = nombreFinal.Length - 1;
							int NumeroFinal = Int32.Parse(nombreFinal.Substring(1, lengthFinal));
							return NumeroFinal;

						}
					}
					return 0;
				}
			}
			catch (Exception ex)
			{

				return 0;
			}

			return 0;
		}

		private void BtnCerrar_Click(object sender, RoutedEventArgs e)
		{
			ini.HiddenTBI();
			Environment.Exit(0);
		}

		private void TxtContrasena_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{

		}
	}
}