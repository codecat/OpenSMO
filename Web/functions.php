<?php
session_start();

include("settings.php");

$database = new PDO("sqlite:".$_SETTINGS['Database']);

function execQuery($q)
{
	global $database;
	$database->exec($q);
}

function sqlQuery($q)
{
	global $database;
	$tmp = $database->query($q);
	return @$tmp->fetchAll();
}

function beginPage()
{
	ob_start();
	
	getSettings();
}

function finishPage()
{
	$out = ob_get_contents();
	ob_clean();
	$out = str_replace("\t", "", $out);
	echo $out;
}

function makeSafeHTML($input)
{
	return htmlentities($input, ENT_QUOTES, "UTF-8");
}

function makeSafeSQL($input)
{
	return str_replace("'", "''", $input);
}

function formatInt($input)
{
	return number_format($input);
}

function me()
{
	if(isLoggedIn())
		return new User(intval($_SESSION['UID']));
	else
		return false;
}

function isLoggedIn()
{
	return isset($_SESSION['UID']);
}

function isAdmin()
{
	global $me;
	return $me->rank == 2;
}

function isMod()
{
	global $me;
	return $me->rank >= 1;
}

function getSessionKey()
{
	return makeSafeHTML($_COOKIE['PHPSESSID']);
}

function echoHiddenSessionkey()
{
	echo "<input type=\"hidden\" name=\"sk\" value=\"".getSessionKey()."\" />";
}

function isValidSessionkey()
{
	return isset($_REQUEST['sk']) && $_REQUEST['sk'] == getSessionKey();
}

function makeBlock($title, $content)
{
	return "<div class=\"block\"><div class=\"blocktitle\">".makeSafeHTML($title)."</div><div class=\"blockcontent\">".$content."</div></div>";
}

function isValidEmail($input)
{
	return preg_match("/^([^\@])\@(.*)\.([a-z]{2,4})$/", $input);
}

function formatString($input)
{
	$colorRegex = "/\|c0([0-9a-fA-F]{6})([^\|c0]+)/";
	
	$parts = array();
	preg_match_all($colorRegex, makeSafeHTML($input), $parts);
	
	$ret = "";
	for($i = 0; $i < count($parts[0]); $i++)
		$ret .= "<span style=\"color:#{$parts[1][$i]}\">{$parts[2][$i]}</span>";
	
	return $ret;
}

function getTopUsers($num, $page = 1)
{
	$res = sqlQuery("SELECT * FROM \"users\" ORDER BY \"XP\" DESC LIMIT ".(($page - 1) * $num).",".intval($num));
	$ret = array();
	//foreach($res as $row)
	foreach($res as $row)
		array_push($ret, new User($row));
	return $ret;
}

function getTopSongs($num, $page = 1)
{
	$res = sqlQuery("SELECT * FROM \"songs\" ORDER BY \"Played\" DESC LIMIT ".(($page - 1) * $num).",".intval($num));
	$ret = array();
	foreach($res as $row)
		array_push($ret, new Song($row));
	return $ret;
}

function getLatestStatsFrom($user, $num, $page = 1)
{
	$res = sqlQuery("SELECT * FROM \"stats\" WHERE \"User\"=".$user->id." ORDER BY \"ID\" DESC LIMIT ".(($page - 1) * $num).",".intval($num));
	$ret = array();
	foreach($res as $row)
		array_push($ret, new Stat($row));
	return $ret;
}

function getLatestStatsFromSong($song, $num, $page = 1)
{
	$res = sqlQuery("SELECT * FROM \"stats\" WHERE \"Song\"=".$song->id." ORDER BY \"ID\" DESC LIMIT ".(($page - 1) * $num).",".intval($num));
	$ret = array();
	foreach($res as $row)
		array_push($ret, new Stat($row));
	return $ret;
}

function getBestStatsFromSong($song, $num, $page = 1)
{
	$res = sqlQuery("SELECT * FROM \"stats\" WHERE \"Song\"=".$song->id." ORDER BY \"Score\" DESC LIMIT ".(($page - 1) * $num).",".intval($num));
	$ret = array();
	foreach($res as $row)
		array_push($ret, new Stat($row));
	return $ret;
}

function getGrade($i)
{
	switch($i)
	{
		case 0: return "AAAA";
		case 1: return "AAA";
		case 2: return "AA";
		case 3: return "A";
		case 4: return "B";
		case 5: return "C";
		case 6: return "D";
	}
	return "F";
}

function getDifficulty($i)
{
	switch($i)
	{
		case 0: return "Beginner";
		case 1: return "Easy";
		case 2: return "Medium";
		case 3: return "Hard";
		case 4: return "Expert";
		case 5: return "Edit";
	}
	return "Edit ".intval($i);
}

function echoStatsTable($stats, $fromSong = false)
{
	?>
	<table>
		<tr>
			<th width="8%">Grade</th>
			<th width="24%"><?php echo ($fromSong ? "Player" : "Song"); ?></th>
			<th width="8%">Flawless</th>
			<th width="8%">Perfect</th>
			<th width="8%">Great</th>
			<th width="8%">Good</th>
			<th width="8%">Barely</th>
			<th width="8%">Miss</th>
			<th width="20%">Player settings</th>
		</tr>
		<?php
		foreach($stats as $stat)
		{
			if($fromSong)
				$stat->fetchUser();
			else
				$stat->fetchSong();
			?>
			<tr>
				<td align="center"><?php $stat->echoGradeImage(); ?></td>
				<?php if($fromSong) { ?>
					<td><?php echo $stat->user->linkName(); ?><br />on: <?php echo getDifficulty($stat->difficulty); ?> (<?php echo $stat->feet; ?>)</td>
				<?php }else{ ?>
					<td><?php echo $stat->song->linkName(); ?><br />by: <?php echo makeSafeHTML($stat->song->artist); ?><br />on: <?php echo getDifficulty($stat->difficulty); ?> (<?php echo $stat->feet; ?>)</td>
				<?php } ?>
				<?php for($i = 8; $i >= 3; $i--) { ?>
				<td align="center"><?php echo $stat->notes[$i]; ?></td>
				<?php } ?>
				<td align="center"><?php echo makeSafeHTML($stat->playersettings); ?></td>
			</tr>
			<?php
		}
		?>
	</table>
	<?php
}

$settings = array();
function getSettings()
{
	global $settings;
	
	$res = sqlQuery("SELECT * FROM \"settings\"");
	foreach($res as $row)
		$settings[$row['Key']] = $row['Value'];
}

function getSetting($key)
{
	global $settings;
	
	if(isset($settings[$key]))
		return $settings[$key];
	return "";
}

function setSetting($key, $value)
{
	global $settings;
	
	if(isset($settings[$key]))
		sqlQuery("UPDATE \"settings\" SET \"Value\"='".makeSafeSQL($value)."' WHERE \"Key\"='".makeSafeSQL($key)."'");
}

function getRoomList()
{
	global $_SETTINGS;
	$url = $_SETTINGS['RTS_IntURL'];
	$buffer = @file_get_contents($url."l");
	if($buffer === false) return false;
	$json = json_decode($buffer);
	$roomlist = array();
	for($i = 0; $i < count($json); $i++) {
		array_push($roomlist, array("ID" => $json[$i][0],
									"Name" => $json[$i][1],
									"Players" => $json[$i][4],
									"Description" => $json[$i][2]));
	}
	return $roomlist;
}
?>