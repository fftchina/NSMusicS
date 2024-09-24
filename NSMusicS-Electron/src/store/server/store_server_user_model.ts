import {reactive, ref, watch} from 'vue'
import {store_app_configs_logic_save} from "@/store/app/store_app_configs_logic_save";
import {store_router_data_info} from "@/store/router/store_router_data_info";
import {store_app_configs_info} from "@/store/app/store_app_configs_info";
import {
    Set_Navidrome_ALL_Data_To_LocalSqlite
} from "@/features/servers_configs/navidrome_api/middleware/class_Set_Navidrome_ALL_Data_To_LocalSqlite";
import {store_server_users} from "@/store/server/store_server_users";
import {
    Get_PlaylistInfo_From_LocalSqlite
} from "@/features/sqlite3_local_configs/class_Get_PlaylistInfo_From_LocalSqlite";
import {store_playlist_list_info} from "@/store/view/playlist/store_playlist_list_info";
import {store_view_media_page_logic} from "@/store/view/media/store_view_media_page_logic";
import {store_view_media_page_fetchData} from "@/store/view/media/store_view_media_page_fetchData";
import {store_router_history_data_of_media} from "@/store/router/store_router_history_data_of_media";
import {store_router_history_data_of_album} from "@/store/router/store_router_history_data_of_album";
import {store_router_history_data_of_artist} from "@/store/router/store_router_history_data_of_artist";
import {store_player_audio_info} from "@/store/player/store_player_audio_info";
import {store_view_album_page_logic} from "@/store/view/album/store_view_album_page_logic";
import {store_view_artist_page_logic} from "@/store/view/artist/store_view_artist_page_logic";
import {store_router_data_logic} from "@/store/router/store_router_data_logic";
import {store_playlist_list_logic} from "@/store/view/playlist/store_playlist_list_logic";
import {store_app_configs_logic_load} from "@/store/app/store_app_configs_logic_load";
import {
    User_Authorization_ApiWebService_of_ND
} from "@/features/servers_configs/navidrome_api/services_web/user_authorization/index_service";

export const store_server_user_model = reactive({
    model_select: 'local',
    server_select: '',
    library_path: '',

    username: '',
    salt: '',
    token: '',
    password: '',

    model_server_type_of_web: true,
    model_server_type_of_local: false,
    authorization: '',
    client_unique_id: '',

    album: 'album',
    annotation: 'annotation',
    artist: 'artist',
    media_file: 'media_file',
    playlist: 'playlist',
    playlist_tracks: 'playlist_tracks',

    switchToMode_Local(){
        this.album = 'album'
        this.annotation = 'annotation'
        this.artist = 'artist'
        this.media_file = 'media_file'
        this.playlist = 'playlist'
        this.playlist_tracks = 'playlist_tracks'
    },
    switchToMode_Navidrome_Api(){
        this.album = 'server_album'
        this.annotation = 'server_annotation'
        this.artist = 'server_artist'
        this.media_file = 'server_media_file'
        this.playlist = 'server_playlist'
        this.playlist_tracks = 'server_playlist_tracks'
    },
    async refresh_model_server_type_of_web(){
        let user_Authorization_ApiWebService_of_ND = new User_Authorization_ApiWebService_of_ND()
        await user_Authorization_ApiWebService_of_ND.get_token()
        store_app_configs_logic_save.save_system_library_config()
    },

    async Get_UserData_Synchronize_ToLocal_of_ND() {
        let set_Navidrome_Data_To_LocalSqlite = new Set_Navidrome_ALL_Data_To_LocalSqlite();
        await set_Navidrome_Data_To_LocalSqlite.Set_Read_Navidrome_Api_PlayListInfo_Add_LocalSqlite(
            store_server_users.server_config_of_current_user_of_sqlite?.url + '/rest',
            store_server_user_model.username, store_server_user_model.token, store_server_user_model.salt,
        );
    },
})
watch(() => store_server_user_model.library_path, (newValue) => {
    store_app_configs_logic_save.save_system_library_config()
    store_router_data_info.router_name = 'View_Song_List_ALL';
    store_app_configs_logic_save.save_system_config_of_View_Router_History()
});
watch(() => store_server_user_model.model_select, async (newValue) => {
    if(!store_app_configs_logic_load.app_configs_loading) {
        // Refresh Playlist(Local / Server)
        await store_playlist_list_logic.reset_data()
        store_playlist_list_info.playlist_MediaFiles_temporary = []
        // Refresh Router Data
        store_router_data_logic.reset_data()
        // Refresh Current AudioInfo
        await store_player_audio_info.reset_data()
        //
        if (store_server_user_model.model_select === 'server') {
            store_server_users.percentage_of_nd = 100
            store_server_users.percentage_of_local = 0
            //
            if(store_server_user_model.model_server_type_of_local){
                store_router_data_info.store_router_history_data_of_local = true
                store_router_data_info.store_router_history_data_of_web = false
            }else if(store_server_user_model.model_server_type_of_web){
                store_router_data_info.store_router_history_data_of_local = false
                store_router_data_info.store_router_history_data_of_web = true
            }
        } else {
            store_server_users.percentage_of_nd = 0
            store_server_users.percentage_of_local = 100
            //
            store_server_user_model.model_server_type_of_local = true
            store_server_user_model.model_server_type_of_web = false
            //
            store_router_data_info.store_router_history_data_of_local = true
            store_router_data_info.store_router_history_data_of_web = false
        }
        store_app_configs_logic_save.save_system_config_of_App_Configs()
    }
});
watch(() => store_server_user_model.model_server_type_of_web, (newValue) => {
    store_server_user_model.model_server_type_of_local = !newValue
    if(store_server_user_model.model_server_type_of_web) {
        store_router_data_info.store_router_history_data_of_local = false
        store_router_data_info.store_router_history_data_of_web = true
    }else{
        store_router_data_info.store_router_history_data_of_local = true
        store_router_data_info.store_router_history_data_of_web = false
    }
    store_app_configs_logic_save.save_system_library_config()
});
watch(() => store_server_user_model.model_server_type_of_local, (newValue) => {
    store_server_user_model.model_server_type_of_web = !newValue
    store_app_configs_logic_save.save_system_library_config()
});