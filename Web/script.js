function htmlSafe(str) {
  return str.replace("<","&lt;").replace(">","&gt;").replace("\"", "&quot;");
}

// Source: http://phpjs.org/functions/number_format:481
function number_format(number, decimals, dec_point, thousands_sep) {
  number = (number + '').replace(/[^0-9+\-Ee.]/g, '');
    var n = !isFinite(+number) ? 0 : +number,
        prec = !isFinite(+decimals) ? 0 : Math.abs(decimals),
    sep = (typeof thousands_sep === 'undefined') ? ',' : thousands_sep,
        dec = (typeof dec_point === 'undefined') ? '.' : dec_point,
        s = '',
        toFixedFix = function (n, prec) {
            var k = Math.pow(10, prec);
      return '' + Math.round(n * k) / k;
        };
    // Fix for IE parseFloat(0.55).toFixed(0) = 0;
    s = (prec ? toFixedFix(n, prec) : '' + Math.round(n)).split('.');
    if (s[0].length > 3) s[0] = s[0].replace(/\B(?=(?:\d{3})+(?!\d))/g, sep);
    if ((s[1] || '').length < prec) {
        s[1] = s[1] || '';
        s[1] += new Array(prec - s[1].length + 1).join('0');
  }
    return s.join(dec);
}

function progressBar(perc) {
  if(perc === false || perc == "100%") return "";
  
  var ret = "";
  ret += "<div class=\"progressbar\">";
  ret += "<div class=\"progressbarfilling\" style=\"width:" + perc + ";\">" + perc + "</div>";
  ret += "</div>";
  return ret;
}

function stepmaniaFormat(str) {
  var lines = str.split("\n");
  var ret = "";
  for(i in lines) {
    var line = lines[i];
    // This line might need reconsideration wether it works good or not...
    ret += "<span class=\"reset\">" + line.replace(/\|c0([a-fA-F0-9]{6})/g, "</span><span style=\"color:#$1;\">") + "</span><br />";
  }
  return ret;
}

function refreshRoomInfo() {
  $.get(rtsURL + "g/" + roomID, function(result) {
    var elemTitle = $("#titleDiv");
    var elemSong = $("#songDiv");
    var elemPlayers = $("#playerDiv");
    var elemChat = $("#chatDiv");
    
    var players = eval(result);
    if(players.length == 0) {
      window.location = "index.php";
    }else{
      var roomInfo = players[0];
      
      elemTitle.innerHTML = htmlSafe(roomInfo[0]) + "<br /><span class=\"desc\">" + htmlSafe(roomInfo[1]) + "</span>";
      if(roomInfo[2] == " - ")
        elemSong.innerHTML = "<b>No song played yet</b>";
      else
        elemSong.innerHTML = progressBar(roomInfo[3]) + htmlSafe(roomInfo[2]);
      
      var chatBuffer = stepmaniaFormat(roomInfo[4]);
      elemChat.innerHTML = chatBuffer;
      
      var htmlBuffer = "<table style=\"width:100%\">";
      htmlBuffer += "<tr>";
      htmlBuffer += "  <th class=\"l\" width=\"20%\">Name</th>";
      htmlBuffer += "  <th class=\"l\" width=\"15%\">Combo</th>";
      htmlBuffer += "  <th class=\"l\" width=\"15%\">Score</th>";
      htmlBuffer += "  <th class=\"l\" width=\"10%\">Grade</th>";
      htmlBuffer += "  <th class=\"l\" width=\"40%\">Settings</th>";
      htmlBuffer += "</tr>";
      for(i = 1; i < players.length; i++) {
        htmlBuffer += "<tr>";
        htmlBuffer += "  <td><img src=\"images/diff_" + htmlSafe(players[i][5].toLowerCase()) + ".png\" /> <a href=\"index.php?page=profile&uid=" + players[i][0] + "\">" + htmlSafe(players[i][1]) + "</a></td>";
        htmlBuffer += "  <td>" + htmlSafe(players[i][2]) + "</td>";
        htmlBuffer += "  <td>" + number_format(players[i][3]) + "</td>";
        htmlBuffer += "  <td><img src=\"images/grade_" + htmlSafe(players[i][4]) + ".png\" /></td>";
        htmlBuffer += "  <td>" + htmlSafe(players[i][6]) + "</td>";
        htmlBuffer += "</tr>";
      }
      htmlBuffer += "</table>";
      elemPlayers.innerHTML = htmlBuffer;
    }
    
    setTimeout(function() {
      refreshRoomInfo();
    }, 500);
  });
}

$.ready(function() {
  if(window.roomID != undefined) {
    $("#chatInput").onkeydown = function(e) {
      if(e.keyCode == 13) {
        $.post("sendchat.php", "r=" + roomID + "&m=" + $("#chatInput").value, function(result) {
          if(result != "OK") {
            alert("Error: " + result);
          }
        });
        $("#chatInput").value = "";
      }
    };
    refreshRoomInfo();
  }
});
