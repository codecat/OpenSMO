<?php

include("functions.php");
include("classes.php");

beginPage();

if(isLoggedIn())
	$me = me();
?>
<!DOCTYPE html>
<html>
	<head>
		<title><?php echo getSetting("Site_Title"); ?></title>
		<link href="style.css" rel="stylesheet" />
		<script src="a2.js"></script>
		<script src="script.js"></script>
		<script>
			var rtsURL = "<?php echo $_SETTINGS['RTS_URL']; ?>";
		</script>
	</head>
	<body>
		<div class="container">
			<div class="header">
				<h1><?php echo getSetting("Site_Title"); ?></h1>
			</div>
			<div class="mid">
				<?php
				if(getSetting("Site_Maintenance") && !(isMod() && getSetting("Site_Maintenance_Staff")) && !isAdmin())
				{
					echo makeBlock("Maintenance", nl2br(getSetting("Site_Maintenance_Message")."<br /><br /><a href=\"index.php?page=login\">Login</a>"));
					if(isset($_GET['page']) && $_GET['page'] == "login")
						include("pages/login.php");
				}else{
					if(getSetting("Site_Maintenance") && ((isMod() && getSetting("Site_Maintenance_Staff")) || isAdmin()))
						echo makeBlock("Maintenance", nl2br(getSetting("Site_Maintenance_Message"))."<br /><br /><b>As you are a staff member, you can still see the website.</b>");
					?>
					<div class="sidebar_left">
						<div class="block">
							<div class="blocktitle">Top 10 players</div>
							<div class="blockcontent">
								<ol>
									<?php
									$topUsers = getTopUsers(10);
									foreach($topUsers as $user)
										echo "<li>".$user->linkName()." (".formatInt($user->xp).")</li>";
									?>
								</ol>
							</div>
						</div>
						<div class="block">
							<div class="blocktitle">Top 10 songs</div>
							<div class="blockcontent">
								<ol>
									<?php
									$topSongs = getTopSongs(10);
									foreach($topSongs as $song)
										echo "<li>".$song->linkName()." (".formatInt($song->played).")</li>";
									?>
								</ol>
							</div>
						</div>
					</div>
					<div class="content">
						<?php
						$page = "home";
						if(isset($_GET['page']))
						{
							if(preg_match("/^([a-z_]+)$/", $_GET['page']))
								$page = $_GET['page'];
						}
						
						$page = str_replace("_", "/", $page);
						
						if(!file_exists("pages/".$page.".php"))
							$page = "404";
						
						include("pages/".$page.".php");
						?>
					</div>
					<div class="sidebar_right">
						<div class="block">
							<div class="blocktitle">Menu</div>
							<div class="blockcontent">
								<a href="index.php">Home</a><br />
							</div>
						</div>
						<?php if($_SETTINGS['RTS_Enabled']) { ?>
						<div class="block">
							<div class="blocktitle">Rooms</div>
							<div class="blockcontent">
								<?php
								$rooms = getRoomList();
								if($rooms === false) {
									echo "<h3 style=\"margin:0px;color:#a00;\">Server Offline!</h3>";
								}else{
									echo "<h3 style=\"margin:0px;color:#0a0;\">Server Online!</h3>";
									foreach($rooms as $room) {
										echo "<p><b><a href=\"index.php?page=room&rid=".$room['ID']."\">".$room['Name']."</a></b> (".$room['Players'].")<br />".$room['Description']."</p>";
									}
								}
								?>
							</div>
						</div>
						<?php } ?>
						<div class="block">
							<div class="blocktitle">Account</div>
							<div class="blockcontent">
								<?php
								if(!isLoggedIn())
								{
									?>
									<a href="index.php?page=login">Login</a><br />
									<a href="index.php?page=register">Register</a><br />
									<?php
								}else{
									?>
									<p class="newsposter">Welcome, <?php echo $me->linkName(); ?>!</p>
									<a href="index.php?page=logout&sk=<?php echo makeSafeHTML($_COOKIE['PHPSESSID']); ?>">Logout</a>
									<?php
								}
								?>
							</div>
						</div>
						<?php
						if(isLoggedIn() && isMod())
						{
							?>
							<div class="block">
								<div class="blocktitle">Moderator menu</div>
								<div class="blockcontent">
									<a href="index.php?page=mod_news">Add news</a><br />
								</div>
							</div>
							<?php
						}
						
						if(isLoggedIn() && isAdmin())
						{
							?>
							<div class="block">
								<div class="blocktitle">Administrator menu</div>
								<div class="blockcontent">
									<a href="index.php?page=admin_settings">Site settings</a>
								</div>
							</div>
							<?php
						}
						?>
					</div>
					<?php
				}
				?>
			</div>
			<div class="footer">
				<p>Powered by <a href="http://opensmo.com/">OpenSMO</a></p>
			</div>
		</div>
	</body>
</html>
<?php finishPage(); ?>