	System.Gadget.settingsUI = "settings.html";
	//===========================contsts=====================
	var default_registry_save_key_path = "HKEY_CURRENT_USER\\SOFTWARE\\WorkTimeAutomaticControl\\Gadget\\CurrentTime";
	var default_save_mins_time_out = 1;
	var default_rest_mins_period_breack = 30;
	var default_rest_mins = 5;
	//===========================vars========================
	var g_date;
	
	var timer_running = false;
	var timer_reseted = true;
	var first_start = true;
	var rest_started = false;
	
	var start_time = 0;
	var pause_time = 0;
	var save_to_file_timer = 0;
	
	var current_minute = 0;
	
	var current_rest_timer = 0;
	
	var shell = new ActiveXObject("WScript.Shell");
	
	function save_current_time_to_registry(ms)
	{	
		shell.RegWrite(default_registry_save_key_path, ms);
	}
	
	function get_current_time_from_registry()
	{	
		try 
		{
			return shell.RegRead(default_registry_save_key_path);
		}
		catch(e)
		{
			save_current_time_to_registry(0)
			return "0";
		}
	}
	
	function timer_update()
	{
		if(timer_running)
		{
			display_time(new Date() - start_time);
			window.setTimeout("timer_update()", 10);
		}
	}
			
	function display_time(ms)
	{
		g_date.setTime(ms);
		
		var s = g_date.getUTCSeconds();
		var m = g_date.getUTCMinutes();
		var h = g_date.getUTCHours();
		var ms100 = parseInt(g_date.getUTCMilliseconds() / 10);
				
		if(s < 10) s = "0" + s;
		if(m < 10) m = "0" + m;
		if(ms100 < 10) ms100 = "0" + ms100;

		if (current_minute != m)
		{
			save_to_file_timer++;
			current_rest_timer++;
			current_minute = m;
		}	
		
		if (save_to_file_timer == default_save_mins_time_out)
		{
			save_to_file_timer = 0;
			save_current_time_to_registry(ms);
		}
		
		if ((current_rest_timer >= default_rest_mins_period_breack) && !rest_started && !first_start)
		{
			rest_started = true;
			current_rest_timer = 0;
			System.Sound.playSound("sounds\\drop_your_weapon.wav");
			shell.Popup("It`s time to get rest for " + default_rest_mins + " mins, after this period you will hear sound alert and you will see an apropriete message.", 15, "Work Time Automatic Control Get Rest Warning", 64);
			document.getElementById("start_pause").innerHTML = "<del>PAUSE</del>";
		}
		else
		{
			if (rest_started)
			{
				if (current_rest_timer >= default_rest_mins)
				{
					rest_started = false;
					current_rest_timer = 0;
					System.Sound.playSound("sounds\\i_have_return.wav");
					shell.Popup("You have rested for " + default_rest_mins + ", now you can safely continue your work.", 15, "Work Time Automatic Control Get Rest Warning", 64);
					document.getElementById("start_pause").innerHTML = "PAUSE";
				}
				else
				{
					document.getElementById("timer").innerHTML = '<span class="fontlarge">'+ (default_rest_mins - current_rest_timer) +'</span><span class="fontsmall"> - countdown</span>';
					return;
				}
			}
		}
		
		if ((m == 55) && (ms100 == 0) && (s == 0))
		{
			System.Sound.playSound("sounds\\input_command.wav");
		}
		
		if(h <= 0)
		{
			document.getElementById("timer").innerHTML = '<span class="fontlarge">' + m + ':' + s + '</span><span class="fontsmall">.' + ms100 + '</span>';
		}
		else
		{
			if(h < 10) h = "0" + h;
				document.getElementById("timer").innerHTML = '<span class="fontlarge">' + h + ':' + m + '</span><span class="fontsmall">:' + s + '</span>';
		}
	}

	function start_pause_timer() 
	{
		if (rest_started)
		{
			return;
		}
		timer_running = !timer_running;
		if(timer_running)
		{
			System.Sound.playSound("sounds\\initiating.wav");
			
			if (timer_reseted)
			{
				g_date = new Date();
				timer_reseted = false;
			}
			
			document.getElementById("start_pause").innerHTML = "PAUSE";
			var date = new Date();
			
			if ((get_current_time_from_registry() != 0) && first_start)
			{
				start_time = date.getTime() - get_current_time_from_registry();
			}
			else
			{
				start_time = date.getTime() - (pause_time - start_time);
			}
			
			first_start = false;
			timer_update();
		}
		else 
		{
			System.Sound.playSound("sounds\\confirmed.wav");
			
			document.getElementById("start_pause").innerHTML = "START";
			
			pause_time = + new Date();
			
			clearTimeout(time_out);
			
			display_time(new Date() - start_time);
		}
				
	}

	function reset_timer() 
	{
		if (!timer_reseted)
		{
			var m = g_date.getUTCMinutes();
			var h = g_date.getUTCHours();
			
			save_current_time_to_registry(0);

			if((((h * 60) + m) >= 15))
			{
				shell.Exec(System.Gadget.path + "\\program\\WorkTimeAutomaticControl.exe " + h + ":" + m);
			}
									
			System.Sound.playSound("sounds\\i_wait_the_instructions.wav");
			
			document.getElementById("timer").innerHTML = '<span class="fontlarge">00:00</span><span class="fontsmall">.00</span>';
			document.getElementById("start_pause").innerHTML = "START";
			
			timer_running = false;
			timer_reseted = true;
			
			start_time = 0;
			pause_time = 0;
			
			current_minute = 0;
			current_rest_timer = 0;
			save_to_file_timer = 0;
			
			clearTimeout(time_out);
		}
	}

	function gadget_init()
	{	
		try 
		{
			if(shell.RegRead("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Client\\Install") != 1)
			{
				shell.Popup(".Net framework v.4 not installed, please visit: http://www.microsoft.com/en-us/download/details.aspx?id=17851 , if you continue using WirkTimeAutomaticControl without .Net framework v.4 all work time will be lost!", 0, "Work Time Automatic Control Critical Error", 16);
			}
		}
		catch(e)
		{
			shell.Popup(".Net framework v.4 not installed, please visit: http://www.microsoft.com/en-us/download/details.aspx?id=17851 , if you continue using WirkTimeAutomaticControl without .Net framework v.4 all work time will be lost!", 0, "Work Time Automatic Control Critical Error", 16);
		}
		finally
		{
			if (timer_reseted)
			{
				g_date = new Date();
				timer_reseted = false;
			}
				
			if ((get_current_time_from_registry() != 0) && first_start)
			{
				start_time = new Date().getTime() - get_current_time_from_registry();
				g_date.setTime(new Date() - start_time);
				current_minute = g_date.getUTCMinutes();
				display_time(new Date() - start_time);
			}
		}		
	}
	