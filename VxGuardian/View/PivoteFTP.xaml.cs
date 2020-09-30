﻿using FluentFTP;
using FubarDev.FtpServer;
using FubarDev.FtpServer.FileSystem.DotNet;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VxGuardian.Common;
using VxGuardian.Models;
using VxGuardian.EtcClass;
using System.Collections;

namespace VxGuardian.View
{
	/// <summary>
	/// Lógica de interacción para ConfiguracionFTP.xaml
	/// </summary>
	public partial class PivoteFTP : Window
	{
		private Inicio ini;
		public FtpClient ftpClient;
		public Timer time;
		public Log gLog;
		public BackgroundWorker worker;

		private VxCMSContext CMSdb;
		private RootCMS rootCMS;

		private string TemporalStorage;
		private BlackScreen bs;
		private Etc tools;
		bool Downloaded = false;
		bool initiated = false;

		public PivoteFTP()
		{
			InitializeComponent();
		}

		public PivoteFTP(Inicio _inicio)
		{
			ini = _inicio;
			InitializeComponent();

			gLog = new Log();
			tools = new Etc();
			//ConnectionDB()
			LoadInitialValuesINICIO();
		}

		public void ConnectionDBCMS(string _jsonFile)
		{
			try
			{
				//db = new VoxContext();
				CMSdb = new VxCMSContext();
				rootCMS = JsonConvert.DeserializeObject<RootCMS>(_jsonFile);
				gLog.SaveLog("Conexion DBCMS Correcta");

			}
			catch (Exception ex)
			{
				gLog.SaveLog("No se pudo conectar base de datos -- " + ex.Message);

				//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
				//Environment.Exit(1);
				//throw;
			}
		}

		public bool CheckErrorResponse(string _link)
		{
			try
			{
				var webClient = new WebClient();
				var jsonPure = webClient.DownloadString(_link);
				JObject json = JObject.Parse(jsonPure);
				if((string) json["error"] == "Not Found"){
					return true;
				}
				return false;


			}
			catch (Exception ex)
			{
				return false;
				//MessageBox.Show("Contexto: " + e.Message + ":" + e.StackTrace);
				//Environment.Exit(1);
				//throw;
			}
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
				TemporalStorage = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VoxLine\\" + ini.config.CodePc;
				Etc.CreateDir(TemporalStorage);
				if (initiated)
				{
					StopTime();
					initiated = false;
				}

				//GUstavo 
				//Crear lock para que nadie mas pueda interactuar.
				/*if (Directory.Exists(ini.config.CarpetaRaiz + ini.config.CodePc))
				{
					Etc.CreateLock(ini.config.CarpetaRaiz + ini.config.CodePc);
					gLog.SaveLog("134 - create lock en la definitiva pivote");
				}else
				{
					Directory.CreateDirectory(ini.config.CarpetaRaiz + ini.config.CodePc);
					gLog.SaveLog("141 - crea carpeta  definitiva pivote");
					Etc.CreateLock(ini.config.CarpetaRaiz + ini.config.CodePc);
					gLog.SaveLog("141 - create lock en la definitiva pivote");
				}*/
								
				Sync();
				//Etc.DeleteLock(ini.config.CarpetaRaiz + ini.config.CodePc);
				//gLog.SaveLog("142 - borra el lock de la definitva pivote");

				if (!initiated)
				{
					InitTime();
					initiated = true;
				}
			




			}
			else
			{
				MessageBox.Show("Los campos obligatorios no se han llenado.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//ini.Close();
			//ini.HiddenTBI();
			//Environment.Exit(0);

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

			
			/*if(!initiated)
			{
				InitTime();
				initiated = true;
			}*/

		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
	
			

			if (ini.config.Syncing == 0)
			{
				if (initiated)
				{
					StopTime();
					initiated = false;
				}


				/*if (Directory.Exists(ini.config.CarpetaRaiz + ini.config.CodePc))
				{
					Etc.CreateLock(ini.config.CarpetaRaiz + ini.config.CodePc);
					gLog.SaveLog("202 - create lock en la definitiva pivote");
				}
				else
				{
					Directory.CreateDirectory(ini.config.CarpetaRaiz + ini.config.CodePc);
					gLog.SaveLog("207 - crea carpeta  definitiva pivote");
					Etc.CreateLock(ini.config.CarpetaRaiz + ini.config.CodePc);
					gLog.SaveLog("209 - create lock en la definitiva pivote");
				}*/

				

				Sync();

				//Etc.DeleteLock(ini.config.CarpetaRaiz + ini.config.CodePc);
				//gLog.SaveLog("142 - borra el lock de la definitva pivote");

				if (!initiated)
				{
					InitTime();
					initiated = true;
				}


			}

			//Application.Current.Dispatcher.Invoke(delegate
			//{
			//	//btn_sync.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			//});
		}


		private void InitTime()
		{
			double interval = Double.Parse(ini.config.TiempoChequeo);
			time = new Timer(Etc.minsToMS(interval));
			time.Elapsed += Timer_Elapsed;
			time.AutoReset = true;
			time.Start();
			Log glog = new Log();
			glog.SaveLog("TIMER :: START ");
		}


		public void StopTime()
		{
			time.Stop();
			Log glog = new Log();
			glog.SaveLog("TIMER :: Stop ");

		}


		private void BtnConfig_Click(object sender, RoutedEventArgs e)
		{
			ini.Show();
			this.Close();
		}

		private void SelectAddress(object sender, RoutedEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				tb.SelectAll();
			}
		}

		private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
		{
			TextBox tb = (sender as TextBox);
			if (tb != null)
			{
				if (!tb.IsKeyboardFocusWithin)
				{
					e.Handled = true;

					tb.Focus();
				}
			}
		}

		public static int DownloadFile(String remoteFilename, String localFilename)
		{
			// Function will return the number of bytes processed
			// to the caller. Initialize to 0 here.
			int bytesProcessed = 0;

			// Assign values to these objects here so that they can
			// be referenced in the finally block
			Stream remoteStream = null;
			Stream localStream = null;
			WebResponse response = null;

			// Use a try/catch/finally block as both the WebRequest and Stream
			// classes throw exceptions upon error
			try
			{
				// Create a request for the specified remote file name
				WebRequest request = WebRequest.Create(remoteFilename);
				if (request != null)
				{
					// Send the request to the server and retrieve the
					// WebResponse object 
					response = request.GetResponse();
					if (response != null)
					{
						// Once the WebResponse object has been retrieved,
						// get the stream object associated with the response's data
						remoteStream = response.GetResponseStream();

						// Create the local file
						localStream = File.Create(localFilename);

						// Allocate a 1k buffer
						byte[] buffer = new byte[1024];
						int bytesRead;

						// Simple do/while loop to read from stream until
						// no bytes are returned
						do
						{
							// Read data (up to 1k) from the stream
							bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

							// Write the data to the local file
							localStream.Write(buffer, 0, bytesRead);

							// Increment total bytes processed
							bytesProcessed += bytesRead;
						} while (bytesRead > 0);
					}
				}
			}
			catch (Exception ex)
			{
				Log glog = new Log();
				glog.SaveLog("No se logro descargar archivos -- " + ex.Message);
			}
			finally
			{
				// Close the response and streams objects here 
				// to make sure they're closed even if an exception
				// is thrown at some point
				if (response != null) response.Close();
				if (remoteStream != null) remoteStream.Close();
				if (localStream != null) localStream.Close();
			}

			// Return total bytes processed to caller.
			return bytesProcessed;
		}


		private void Sync()
		{

			/*if(initiated)
			{
				StopTime();
				initiated = false;
			}*/
			

			var dictionary = new Dictionary<int, int>();
			//descargar json
			using (var webClient = new WebClient())
			{
				//string download link
				string downloadLink = ini.config.ModePivote[0].Servidor + "pivot/" + ini.config.CodePc + "/get/" + ini.config.ModePivote[0].Contrasena;
				gLog.SaveLog("315 - Genera string downloadLink");
				try 
				{
					//leer string json
					var jsonPure = webClient.DownloadString(downloadLink);
					gLog.SaveLog("321 - Se conecta y lee Json CMS");
					//generar db
					ConnectionDBCMS(jsonPure);
					gLog.SaveLog("324 - Genera BD.Json");


					syncingOn();

					//raiz folder
					string rootFolder = ini.config.CarpetaRaiz + rootCMS.Code;
					//crear raiz
					Etc.CreateDir(rootFolder);
					gLog.SaveLog(" 398 - Crea la carpeta raiz : " + rootFolder);
					//------------------------------------------------------------------------------------
					//Daniel
					//Crear archivo Json					
					string json = JsonConvert.SerializeObject(rootCMS);

					//string path2 = @"C:\\FTP\\Voxline\\Pivotes\\35000" +  "/temp6.json";
					string path = rootFolder + "\\PlayList.json";


					gLog.SaveLog("408 - Crea el json en la carpeta raiz");
					//Gustavo
					if(File.Exists(path))
					{
						File.Delete(path);
						gLog.SaveLog("342 - File.Delete " + path);
						File.WriteAllText(path, json);
					}else
					{
						System.IO.File.WriteAllText(path, json);
					}


					//------------------------------------------------------------------------------------
					

					//recorrer computador
					 foreach (var computer in rootCMS.Computers)
					{
						//computador string
						string computerFolder = rootFolder + "\\" + computer.Code;
						// crear string computador
						gLog.SaveLog("430 - crea carpeta por computador  : " + computer.Code);

						if(!(Directory.Exists(computerFolder)))
						{
							Etc.CreateDir(computerFolder);
						}
						


						///////////////GUSTAVO
						/*Etc.CreateLock((computerFolder));
						gLgCreateLock.SaveLog("432 - Creando lock en computer folder : " + computerFolder); */


						//auxiliar
						int aux = 0;
						foreach (Screens screen in computer.Screens)
						{
							
							
							//URLL carpetas pantalla
							string screenFolder = computerFolder + "\\p" + screen.Code;
							string screenFolder_TMP = computerFolder + "\\p" + screen.Code +"_TMP";

							if (!(Etc.CheckLock(screenFolder)))
							{
								gLog.SaveLog("447 La pantalla se encuentra disponible siguer su curso");
								//chequear si existe archivo lock
								gLog.SaveLog("445 - antes del etc.checklock(screenfolder_tmp)");
								if (!Etc.CheckLock(screenFolder_TMP))
								{
									gLog.SaveLog("448 - despues del etc.checklock(screenfolder_tmp)");
									//crear carpeta pantalla
									if (!(Directory.Exists(screenFolder)))
									{
										//Si el diecotrio principal no existe lo crea
										Etc.CreateDir(screenFolder);
										gLog.SaveLog("405 Crea carpeta definitva pantalla :  " + screenFolder);
									}

									//CREA lock por pantalla
									Etc.CreateLock(screenFolder);
									gLog.SaveLog("LOCK creado por pantalla : " + screenFolder);



									/*Etc.CreateDir(screenFolder_TMP);
									gLog.SaveLog("375 - Etc.CreateDir " + screenFolder_TMP); */

									//crear localmente la variable de la pantalla
									ScreensGuardian _screen = new ScreensGuardian();
									//primero?
									bool first = false;


									_screen = ini.config.Screens.Find(e => e.Code == screen.Code.ToString());

									if (_screen == null)
									{
										//Esto no se ocupa ? 
										_screen = new ScreensGuardian();
										_screen.Alto = screen.Height.ToString();
										_screen.Ancho = screen.Width.ToString();
										_screen.Code = screen.Code.ToString();
										_screen.Nombre = screen.Name;
										_screen.VersionActual = "0";
										//_screen.VersionActual = (screen.Version-1).ToString();//cambio gonzalo
										//string versionValidate = screenFolder + "/" + "v" + screen.Version.ToString() + ".txt";//cambio gonzalo
										//cambio gonzalo
										//if (File.Exists(versionValidate))
										//{
										//	_screen.VersionActual = screen.Version.ToString();
										//}
										//cambio gonzalo
										//if (Etc.CheckEmptyFolder(screenFolder))
										//{
										//	first = true;
										//}

										first = true;
										ini.config.Screens.Add(_screen);
										ini.db.Save(ini.config);
										//Etc.ClearDir(screenFolder);
										//gLog.SaveLog("414 - Etc.ClearDir " + screenFolder);
									}


									//si se asigno recientemente
									if (first)
									{
										//Etc.ClearDir(screenFolder_TMP);
										//gLog.SaveLog("421 - Etc.ClearDir " + screenFolder_TMP);
										//crear carpeta pantalla

										if (Directory.Exists(screenFolder_TMP))
										{
											//SI existe la carpeta temporal la limpia
											Etc.ClearDir(screenFolder_TMP);
											gLog.SaveLog("459 - Limpia temporal " + screenFolder_TMP);
										}
										else
										{
											//Crear carpeta temporal									
											Etc.CreateDir(screenFolder_TMP);
											gLog.SaveLog("465 - Crea temporal " + screenFolder_TMP);
										}




										Etc.CreateLock(screenFolder_TMP);
										//Etc.CreateLock(screenFolder);
										int auxI = 0;
										if (screen.Playlist != null)
										{
											foreach (var content in screen.Playlist)
											{
												try
												{


													if (!CheckErrorResponse(content.Download.ToString()))
													{

														//  string contentName = content.OriginalID + "-"+ content.defOrder + content.Name + "-" + auxI + ".mp4";


														string contentName = content.OriginalID + "-" + content.Name + ".mp4";

														//Daniel
														//Comparo si el contenName existe en la carpeta de descarga si es asi no lo descargo.
														if (Etc.CheckFile(screenFolder + "\\" + contentName))
														{
															File.Copy(screenFolder + "\\" + contentName, screenFolder_TMP + "\\" + contentName);
															gLog.SaveLog("First: Se copio a la temporal " + contentName + " en " + screenFolder_TMP);
														}
														else
														{
															Etc.CreateDir(screenFolder_TMP);  // Daniel
															DownloadFile(content.Download.ToString(), screenFolder_TMP + "\\" + contentName);// Daniel
																																			 //DownloadFile(content.Download.ToString(), screenFolder + "/" + contentName);
															gLog.SaveLog("First: Se descargo a la temporal" + contentName + " en " + screenFolder_TMP);
															auxI++;
														}


													}
												}
												catch (Exception ex)
												{
													gLog.SaveLog("First: No se logro descargar archivos a la temporal-- " + ex.Message);
													//throw;
												}
											}// End foreach cargar contenido a la temporal

											//GUSTAVO 
											//Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel									
											//Etc.DeleteLock(screenFolder_TMP); // Daniel
											//Etc.MoveDir(screenFolder_TMP, screenFolder);
											/////////////Directory.Delete(screenFolder, true); // Daniel
											/////////////gLog.SaveLog("468 - FIRST : Directory.Delete " + screenFolder); 


										}
										else
										{
											gLog.SaveLog("First: Sin Contenido asignado");
											//Etc.DeleteLock(screenFolder_TMP);
											//////////////Directory.Delete(screenFolder_TMP, true);
											//////////gLog.SaveLog("474 - FIRST : Directory.Delete " + screenFolder_TMP);

											if (Etc.CheckDir(screenFolder))
											{
												Etc.DeleteFiles(screenFolder);
												gLog.SaveLog("477 - FIRST : Etc.DeleteFile " + screenFolder_TMP);


											}
										}

										/* CAIDA
										Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel
										Directory.Delete(screenFolder,true); // Daniel
										Etc.DeleteLock(screenFolder_TMP); // Daniel
										Etc.MoveDir(screenFolder_TMP, screenFolder); */


										//Etc.CreateVersion(screenFolder, screen.Version.ToString());
										//Etc.DeleteLock(screenFolder);

										//Etc.DeleteVersion(screenFolder, _screen.VersionActual);
										//Etc.CreateVersion(screenFolder, screen.Version.ToString());


										_screen.VersionActual = screen.Version.ToString();
										ini.config.Screens[aux] = _screen;
										ini.db.Save(ini.config);
										Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel								
										Etc.DeleteLock(screenFolder_TMP); // Daniel
										gLog.SaveLog("500 - FIRST : Etc.DeleteLock " + screenFolder_TMP);
										Etc.DeleteLock(screenFolder);
										gLog.SaveLog("619 - FIRST Borra el lock de la pantalla antes de mover la temporal a la definitiva Etc.DeleteLock" + screenFolder);
										Etc.MoveDir(screenFolder_TMP, screenFolder);
										gLog.SaveLog("620 - FIRST : Contenido movido de la temporal " + screenFolder_TMP + " a la definitiva " + screenFolder);

										first = false;
									}
									//si la version
									else if (screen.Version > Int32.Parse(_screen.VersionActual))
									{
										//gustavo
										//_screen.VersionActual = screen.Version.ToString();
										////------------------------------------------------------------------------------------
										////Daniel
										////Crear archivo Json
										//string json = JsonConvert.SerializeObject(rootCMS);
										//string path = computerFolder + "/PlayList.json";
										//System.IO.File.WriteAllText(path, json);
										////------------------------------------------------------------------------------------
										gLog.SaveLog("Version Actual " + _screen.VersionActual + " -- Version Remota " + screen.Version);
										//Etc.DeleteFiles(screenFolder);//cambio gonzalo
										//Etc.ClearDir(screenFolder);
										//Etc.CreateLock(screenFolder_TMP);
										// Daniel
										if (Directory.Exists(screenFolder_TMP))
										{
											//SI existe la carpeta temporal la limpia
											Etc.ClearDir(screenFolder_TMP);
											gLog.SaveLog("585 - Limpia temporal " + screenFolder_TMP);
										}
										else
										{
											//Crear carpeta temporal									
											Etc.CreateDir(screenFolder_TMP);
											gLog.SaveLog("590 - Crea temporal " + screenFolder_TMP);
										}



										//	Etc.ClearDir(screenFolder_TMP);
										gLog.SaveLog("523 - Etc.ClearDir" + screenFolder_TMP);
										Etc.CreateLock(screenFolder_TMP);
										gLog.SaveLog("525 - CreateLock" + screenFolder_TMP);
										//

										int auxI = 0;
										if (screen.Playlist != null)
										{
											foreach (var content in screen.Playlist)
											{
												try
												{
													if (!CheckErrorResponse(content.Download.ToString()))
													{
														//string contentName = content.defOrder + content.Name + "-" + auxI + ".mp4";
														//Daniel  
														string contentName = content.OriginalID + "-" + content.Name + ".mp4";
														//Daniel


														if (Etc.CheckFile(screenFolder + "\\" + contentName))
														{
															File.Copy(screenFolder + "\\" + contentName, screenFolder_TMP + "\\" + contentName);
															gLog.SaveLog("Se copio a la temporal " + contentName + " en " + screenFolder);
														}
														else
														{
															DownloadFile(content.Download.ToString(), screenFolder_TMP + "\\" + contentName);
															gLog.SaveLog("Se descargo a la temporal " + contentName + " en " + screenFolder);
															auxI++;
														}


													}
												}
												catch (Exception ex)
												{
													gLog.SaveLog("No se logro descargar archivos a la temporal -- " + ex.Message);
													//throw;
												}
											}//end foreach
											Directory.Delete(screenFolder, true); // Daniel
										}
										else
										{
											gLog.SaveLog("Sin Contenido asignado");
											//Daniel

											Etc.DeleteLock(screenFolder_TMP);
											gLog.SaveLog("522 - Etc.DeleteLock" + screenFolder_TMP);
											Directory.Delete(screenFolder_TMP, true);
											gLog.SaveLog("573 - Etc.ClearDir" + screenFolder_TMP);

											if (Etc.CheckDir(screenFolder))
											{
												Etc.DeleteFiles(screenFolder);
												gLog.SaveLog("Contenido borrado de la carpeta definitiva");
											}
											//-------------------

										}


										//Etc.DeleteVersion(screenFolder, _screen.VersionActual);
										//Etc.CreateVersion(screenFolder, screen.Version.ToString());
										_screen.VersionActual = screen.Version.ToString();
										ini.config.Screens[aux] = _screen;
										ini.db.Save(ini.config);

										//GUSTAVO
										Etc.CreateVersion(screenFolder_TMP, screen.Version.ToString()); //Daniel								
										gLog.SaveLog("592 - Etc.CreateVersion" + screenFolder_TMP + " " + screen.Version.ToString());
										Etc.DeleteLock(screenFolder_TMP); // Daniel
										gLog.SaveLog("594 - Etc.DeleteLock" + screenFolder_TMP);
										Etc.DeleteLock(screenFolder);
										gLog.SaveLog("732 - Borra el lock de la pantalla antes de mover la temporal a la definitiva Etc.DeleteLock" + screenFolder);

										Etc.MoveDir(screenFolder_TMP, screenFolder);
										gLog.SaveLog("597 - Contenido movido de latemporal a la definitiva");
										///////////////////


										//Etc.DeleteLock(screenFolder);
									}
									else
									{
										gLog.SaveLog("758 - No hay cambio de version");
										Etc.DeleteLock(screenFolder_TMP); // Daniel
										gLog.SaveLog("594 - Etc.DeleteLock" + screenFolder_TMP);
										Etc.DeleteLock(screenFolder);
										gLog.SaveLog("732 - Borra el lock de la pantalla antes de mover la temporal a la definitiva Etc.DeleteLock" + screenFolder);
									}//Fin if comprueba version

								}

								//Posible falla
								if (Etc.CheckDir(screenFolder_TMP))
								{
									///////////////Directory.Delete(screenFolder_TMP, true); // Daniel
									/////////////gLog.SaveLog("609 - Delete " + screenFolder_TMP);

								}
								aux++;
							}else
							{
								gLog.SaveLog(" 761 - la carpeta se encuentra ocupada , se encontro LOCK.TXT  pantalla : "+ screenFolder);
							}
						} //Fin foreach descargar contenido y mover a la definitiva

						//Etc.DeleteLock(computerFolder);
						//gLog.SaveLog("Borrando lock de computer folder : " + computerFolder );
						syncingOff();

					}
				}
				catch (Exception ex)
				{
					gLog.SaveLog("634 - No se logro sincronizar archivos -- " + ex.Message);

					InitTime();
					initiated = true;
					//throw;
				}



			}
		}



		public void LoadInitialValuesINICIO()
		{
			TxtCodigo.Text = ini.config.CodePc;
			TxtRaizSel.Text = ini.config.CarpetaRaiz;
			TxtChequeo.Text = ini.config.TiempoChequeo;
			TxtCodigoMaestro.Password = ini.config.ModePivote[0].Contrasena;
			TxtServidor.Text = ini.config.ModePivote[0].Servidor;
		}

		public Boolean CheckFieldsINICIAR()
		{
			if (
					!Etc.CheckFieldsTBOX(TxtCodigo) ||
					!Etc.CheckFieldsTBOX(TxtRaizSel) ||
					!Etc.CheckFieldsTBOX(TxtChequeo) ||
					!Etc.CheckFieldsTBOX(TxtServidor) ||
					!Etc.CheckFieldsPBOX(TxtCodigoMaestro)
				)
			{
				return false;
			}
			return true;
		}

		public void SaveNewConfig()
		{
			ini.config.CodePc = TxtCodigo.Text.Trim();
			ini.config.CarpetaRaiz = TxtRaizSel.Text.Trim();
			ini.config.TiempoChequeo = TxtChequeo.Text.Trim();
			ini.config.ModePivote[0].Servidor = TxtServidor.Text.Trim();
			ini.config.ModePivote[0].Contrasena = TxtCodigoMaestro.Password.Trim();
			ini.config.SelectedMode = "Pivote";
			ini.db.Save(ini.config);
		}

		// Event to track the progress
		void wc_progressBar(object sender, DownloadProgressChangedEventArgs e)
		{
			//progressBar.Value = e.ProgressPercentage;
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

		private void BtnCerrar_Click(object sender, RoutedEventArgs e)
		{
			ini.HiddenTBI();
			Environment.Exit(0);
		}
	}
}
